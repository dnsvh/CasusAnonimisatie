using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController I;

    [Header("Data & Settings")]
    public TextAsset suspectsJson; // assign Resources load in Awake if empty
    public AnonymizationSettings settings;
    public GameMode mode = GameMode.Moderate;

    [Header("HUD")]
    public HudController hud;

    private List<Suspect> all;
    private List<Suspect> candidates;
    private List<(string key, string value)> activeFilters = new();

    private void Awake()
    {
        I = this;
        if (suspectsJson == null)
            suspectsJson = Resources.Load<TextAsset>("Data/suspects");

        var wrap = JsonUtility.FromJson<SuspectsWrapper>(suspectsJson.text);
        all = wrap.items;
        candidates = new List<Suspect>(all);

        // default presets
        ApplyPreset(mode);
        RecomputeMetricsAndHud();
    }

    public void ApplyPreset(GameMode m)
    {
        // quick defaults; later make 4 ScriptableObject assets
        switch (m)
        {
            case GameMode.Easy:
                settings.ageBucket = 1; settings.geoLevel = AnonymizationSettings.GeoLevel.Postcode;
                settings.jobLevel = AnonymizationSettings.JobLevel.Exact; settings.useNoise = false;
                settings.kTarget = 1; settings.lTarget = 1; settings.tThreshold = 0.5f;
                break;
            case GameMode.Moderate:
                settings.ageBucket = 10; settings.geoLevel = AnonymizationSettings.GeoLevel.District;
                settings.jobLevel = AnonymizationSettings.JobLevel.Sector; settings.useNoise = false;
                settings.kTarget = 5; settings.lTarget = 2; settings.tThreshold = 0.25f;
                break;
            case GameMode.Hard:
                settings.ageBucket = 20; settings.geoLevel = AnonymizationSettings.GeoLevel.Province;
                settings.jobLevel = AnonymizationSettings.JobLevel.Sector; settings.useNoise = true; settings.epsilon = 0.5f;
                settings.kTarget = 10; settings.lTarget = 3; settings.tThreshold = 0.2f;
                break;
            case GameMode.Custom:
                // leave as-is; controlled by UI
                break;
        }
    }

    // Called by NPC after player presses E
    public void OnClueReceived(RawClue raw)
    {
        // Convert raw -> anonymized filter string pair
        var filter = AnonymizeRawClue(raw);
        activeFilters.Add(filter);
        FilterCandidates();
        RecomputeMetricsAndHud();

        // show popup in HUD dialogue area
        if (hud) hud.ShowClueChip(filter.key, filter.value);
    }

    (string key, string value) AnonymizeRawClue(RawClue raw)
    {
        // Minimal set: Age, District, Sector (you can extend)
        if (raw.field == ClueField.Age)
        {
            int age = int.Parse(raw.value);
            if (settings.useNoise) age = Anonymizer.AddTinyNoiseToAge(age, settings.epsilon);
            int start = Anonymizer.ToAgeBucketStart(age, settings.ageBucket);
            return ("Age", Anonymizer.FormatAgeBucket(start, settings.ageBucket));
        }
        if (raw.field == ClueField.District)
        {
            int d = int.Parse(raw.value);
            // geo generalization demo uses district label
            return ("Geo", settings.geoLevel == AnonymizationSettings.GeoLevel.Postcode ? $"Postcode {d}" : $"District {d}");
        }
        if (raw.field == ClueField.Sector)
        {
            // job generalization (Exact vs Sector)
            return ("Job", raw.value);
        }
        // fallback
        return (raw.field.ToString(), raw.value);
    }

    void FilterCandidates()
    {
        candidates = all.Where(s => MatchesAllFilters(s)).ToList();
    }

    bool MatchesAllFilters(Suspect s)
    {
        foreach (var f in activeFilters)
        {
            switch (f.key)
            {
                case "Age":
                    if (!AgeBucketMatch(s.age, f.value)) return false; break;
                case "Geo":
                    if (settings.geoLevel == AnonymizationSettings.GeoLevel.District)
                    {
                        if (f.value.StartsWith("District "))
                        {
                            int want = int.Parse(f.value.Substring(9));
                            if (s.district != want) return false;
                        }
                    }
                    // (Postcode/Province omitted for brevity)
                    break;
                case "Job":
                    string job = settings.jobLevel == AnonymizationSettings.JobLevel.Sector ? s.sector : s.occupation;
                    if (job != f.value) return false;
                    break;
            }
        }
        return true;
    }

    bool AgeBucketMatch(int age, string bucketText)
    {
        if (!bucketText.Contains("-")) return age.ToString() == bucketText;
        var parts = bucketText.Split('-');
        int a = int.Parse(parts[0]); int b = int.Parse(parts[1]);
        return age >= a && age <= b;
    }

    void RecomputeMetricsAndHud()
    {
        // Build quasi-ID tuples for current candidates
        var rows = candidates.Select(s => (
            ageBucket: Anonymizer.ToAgeBucketStart(settings.useNoise ? Anonymizer.AddTinyNoiseToAge(s.age, settings.epsilon) : s.age, settings.ageBucket),
            gender: s.gender,
            geo: settings.geoLevel == AnonymizationSettings.GeoLevel.District ? $"District {s.district}" : (settings.geoLevel==AnonymizationSettings.GeoLevel.Postcode? s.postcode : "Province X"),
            job: settings.jobLevel == AnonymizationSettings.JobLevel.Sector ? s.sector : s.occupation
        ));

        int k = Metrics.ComputeK(rows);
        int l = Metrics.ComputeL(candidates.Select(s => s.sensitive_condition));
        float t = Metrics.ComputeTCloseness(candidates.Select(s=>s.sensitive_condition), all.Select(s=>s.sensitive_condition));

        if (hud) hud.UpdateHud(candidates.Count, k, l, t, settings);
    }

    public bool TryAccuse(Suspect chosen)
    {
        bool win = chosen.is_murderer;
        // Show a simple result; you can expand
        if (hud) hud.ShowResult(win);
        return win;
    }

    public List<Suspect> GetCandidates() => candidates;
}
