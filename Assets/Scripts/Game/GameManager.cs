using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public GameSettings settings;
    public RoundPhase phase = RoundPhase.RoundA;

    public float roundTimer = 0f;

    // Tijden per ronde + laatst bekende totaaltijd 
    public float roundASeconds = 0f;
    public float roundBSeconds = 0f;
    public float lastTotalSeconds = 0f;

    public SuspectDataset datasetA;
    public SuspectDataset datasetB;

    [SerializeField] private string killerIdA = null;
    [SerializeField] private string killerIdB = null;

    private readonly HashSet<ClueType> usedCluesA = new HashSet<ClueType>();
    private readonly HashSet<ClueType> usedCluesB = new HashSet<ClueType>();

    public bool lastAccuseCorrect = false;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (settings == null)
            settings = Resources.Load<GameSettings>("Data/GameSettings");

        var dsAFromJson = DataService.LoadFromResources<SuspectDataset>("Data/hardcoded_suspects");
        var dsBFromJson = DataService.LoadFromResources<SuspectDataset>("Data/hardcoded_suspects_b");

        if (dsAFromJson != null && dsAFromJson.suspects != null && dsAFromJson.suspects.Count > 0)
            datasetA = EnsureIdsAndClean(dsAFromJson);
        else if (datasetA == null || datasetA.suspects == null || datasetA.suspects.Count == 0)
            datasetA = GenerateSynthetic(30); 

        if (dsBFromJson != null && dsBFromJson.suspects != null && dsBFromJson.suspects.Count > 0)
            datasetB = EnsureIdsAndClean(dsBFromJson);
        else if (datasetB == null || datasetB.suspects == null || datasetB.suspects.Count == 0)
            datasetB = CloneDataset(datasetA); 

        // Ensure Round A killer
        if (string.IsNullOrEmpty(killerIdA) && datasetA.suspects.Count > 0)
            killerIdA = datasetA.suspects[UnityEngine.Random.Range(0, datasetA.suspects.Count)].id;

        // Ensure Round B killer 
        if ((string.IsNullOrEmpty(killerIdB) || killerIdB == killerIdA) && datasetB.suspects.Count > 0)
        {
            if (datasetB.suspects.Count > 1)
            {
                var pool = datasetB.suspects.Where(s => s.id != killerIdA).ToList();
                var pick = pool.Count > 0 ? pool[UnityEngine.Random.Range(0, pool.Count)]
                                          : datasetB.suspects[0];
                killerIdB = pick.id;
            }
            else
            {
                killerIdB = datasetB.suspects[0].id;
            }
        }

        Debug.Log($"[GM] Awake. Phase={phase}, killerIdA={killerIdA}, killerIdB={killerIdB}");
    }

    public void TickTimer(float dt) => roundTimer += dt;

    public SuspectDataset CurrentDataset() => (phase == RoundPhase.RoundB) ? datasetB : datasetA;

    public IEnumerable<Suspect> RemainingSuspects()
        => CurrentDataset().suspects.Where(s => !s.eliminated);

    public Suspect GetKiller()
        => (phase == RoundPhase.RoundB)
            ? datasetB?.suspects?.FirstOrDefault(s => s.id == killerIdB)
            : datasetA?.suspects?.FirstOrDefault(s => s.id == killerIdA);

    public Suspect GetKillerOfRoundA() => datasetA?.suspects?.FirstOrDefault(s => s.id == killerIdA);
    public Suspect GetKillerOfRoundB() => datasetB?.suspects?.FirstOrDefault(s => s.id == killerIdB);

    void EnsureKillerSelectedForCurrentRound()
    {
        if (phase == RoundPhase.RoundA)
        {
            if (string.IsNullOrEmpty(killerIdA) && datasetA?.suspects?.Count > 0)
                killerIdA = datasetA.suspects[UnityEngine.Random.Range(0, datasetA.suspects.Count)].id;
        }
        else
        {
            if (string.IsNullOrEmpty(killerIdB) && datasetB?.suspects?.Count > 0)
            {
                var pool = (datasetB.suspects.Count > 1)
                    ? datasetB.suspects.Where(s => s.id != killerIdA).ToList()
                    : datasetB.suspects.ToList();

                var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
                killerIdB = pick.id;
            }
        }
    }

    void EnsureKillerSelected() => EnsureKillerSelectedForCurrentRound();

    public void EliminateByClue(Func<Suspect, bool> keepPredicate)
    {
        var data = CurrentDataset();
        foreach (var s in data.suspects)
            if (!keepPredicate(s))
                s.eliminated = true;
    }

    public void ResetEliminationsForCurrentRound()
    {
        foreach (var s in CurrentDataset().suspects)
            s.eliminated = false;
    }

    public void ResetToRoundA()
    {
        phase = RoundPhase.RoundA;
        roundTimer = 0f;
        roundASeconds = 0f;
        roundBSeconds = 0f;
        lastTotalSeconds = 0f;
        usedCluesA.Clear();
        foreach (var s in datasetA.suspects) s.eliminated = false;

        if (string.IsNullOrEmpty(killerIdA) && datasetA.suspects.Count > 0)
            killerIdA = datasetA.suspects[UnityEngine.Random.Range(0, datasetA.suspects.Count)].id;
    }

    public void BeginRoundB()
    {
        // eindtijd A vastleggen
        roundASeconds = roundTimer;

        phase = RoundPhase.RoundB;
        roundTimer = 0f;
        usedCluesB.Clear();
        foreach (var s in datasetB.suspects) s.eliminated = false;

        if (datasetB?.suspects != null && datasetB.suspects.Count > 0)
        {
            if (datasetB.suspects.Count > 1)
            {
                var pool = datasetB.suspects.Where(s => s.id != killerIdA).ToList();
                var pick = pool.Count > 0 ? pool[UnityEngine.Random.Range(0, pool.Count)]
                                          : datasetB.suspects[0];
                killerIdB = pick.id;
            }
            else
            {
                killerIdB = datasetB.suspects[0].id;
            }
        }

        Debug.Log($"[GM] BeginRoundB. killerIdA={killerIdA}, killerIdB={killerIdB}");
    }

    // Accuse flow: correct in A -> naar B; incorrect in A -> Start
    // In B -> Debrief (win/lose)
    public void Accuse(string suspectId)
    {
        EnsureKillerSelectedForCurrentRound();

        var killer = GetKiller();
        var expectedId = (phase == RoundPhase.RoundB) ? killerIdB : killerIdA;
        bool correct = (killer != null && suspectId == expectedId);

        Debug.Log($"[GM] Accuse called with suspectId={suspectId}");
        Debug.Log($"[GM] Killer is {killer?.name} ({killer?.id}) in phase {phase}");

        if (phase == RoundPhase.RoundA)
        {
            if (correct)
            {
                lastAccuseCorrect = true;
                Debug.Log("[GM] Accuse: CORRECT (Round A) -> Round B");
                BeginRoundB(); // zet roundASeconds
                SceneManager.LoadScene("TheCityAnon");
            }
            else
            {
                lastAccuseCorrect = false;
                roundASeconds = roundTimer;
                lastTotalSeconds = roundASeconds;
                Debug.Log("[GM] Accuse: WRONG (Round A) -> Start");
                SceneManager.LoadScene("Start");
            }
        }
        else
        {
            roundBSeconds = roundTimer;
            lastTotalSeconds = Mathf.Max(0f, roundASeconds) + Mathf.Max(0f, roundBSeconds);

            if (correct)
            {
                lastAccuseCorrect = true;
                Debug.Log("[GM] Accuse (Round B): CORRECT. To Debrief.");
            }
            else
            {
                lastAccuseCorrect = false;
                Debug.Log("[GM] Accuse (Round B): WRONG. To Debrief.");
            }
            SceneManager.LoadScene("Debrief");
        }
    }

    public ClueType ChooseClueType(ClueType preferred)
    {
        var used = (phase == RoundPhase.RoundB) ? usedCluesB : usedCluesA;
        bool Age(ClueType t) => t == ClueType.AgeExact || t == ClueType.AgeRange;
        bool anyAgeUsed = used.Contains(ClueType.AgeExact) || used.Contains(ClueType.AgeRange);

        if (!used.Contains(preferred) && (!Age(preferred) || !anyAgeUsed))
            return preferred;

        var all = new ClueType[] {
            ClueType.Gender, ClueType.District, ClueType.Postcode2, ClueType.Occupation,
            ClueType.AgeExact, ClueType.AgeRange
        };
        foreach (var t in all)
            if (!used.Contains(t) && (!Age(t) || !anyAgeUsed))
                return t;

        foreach (var t in new ClueType[] { ClueType.AgeExact, ClueType.AgeRange })
            if (!used.Contains(t))
                return t;

        return preferred;
    }

    public void MarkClueUsed(ClueType t)
    {
        var used = (phase == RoundPhase.RoundB) ? usedCluesB : usedCluesA;

        if (t == ClueType.AgeExact || t == ClueType.AgeRange)
        {
            used.Add(ClueType.AgeExact);
            used.Add(ClueType.AgeRange);
            return;
        }
        used.Add(t);
    }

    private SuspectDataset EnsureIdsAndClean(SuspectDataset ds)
    {
        if (ds?.suspects == null) return ds;
        var seen = new HashSet<string>();
        foreach (var s in ds.suspects)
        {
            if (string.IsNullOrWhiteSpace(s.id))
                s.id = Guid.NewGuid().ToString("N").Substring(0, 8);
            if (seen.Contains(s.id))
                s.id = Guid.NewGuid().ToString("N").Substring(0, 8);
            seen.Add(s.id);

            // reset eliminations bij start
            s.eliminated = false;

            if (string.IsNullOrWhiteSpace(s.district))
                s.district = "Onbekend";
        }
        return ds;
    }

    // Genereer synthetische data met provincies + postcode "1111AA"
    private SuspectDataset GenerateSynthetic(int count)
    {
        var rnd = new System.Random();
        var ds = new SuspectDataset();

        string[] genders = { "M", "F" };
        // We gebruiken het bestaande veld 'district' om een provincie-naam op te slaan
        string[] provinces = {
            "Groningen","Friesland","Drenthe","Overijssel","Flevoland","Gelderland",
            "Utrecht","Noord-Holland","Zuid-Holland","Zeeland","Noord-Brabant","Limburg"
        };
        string[] occupations = { "Bakker", "Leraar", "Student", "Arts", "Chauffeur", "Programmeur" };

        for (int i = 0; i < count; i++)
        {
            var s = new Suspect
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 8),
                name = $"Persoon{i + 1}",
                age = rnd.Next(18, 70),
                gender = genders[rnd.Next(genders.Length)],
                district = provinces[rnd.Next(provinces.Length)],   // provincie in 'district'
                postcode = GenerateDutchPostcode(rnd),              
                occupation = occupations[rnd.Next(occupations.Length)],
                eliminated = false
            };
            ds.suspects.Add(s);
        }
        return ds;
    }

    private static string GenerateDutchPostcode(System.Random rnd)
    {
        int n = rnd.Next(1000, 10000); // 1000..9999
        char a = (char)('A' + rnd.Next(0, 26));
        char b = (char)('A' + rnd.Next(0, 26));
        return $"{n}{a}{b}";
    }

    private SuspectDataset CloneDataset(SuspectDataset src)
    {
        var ds = new SuspectDataset();
        foreach (var s in src.suspects)
        {
            ds.suspects.Add(new Suspect
            {
                id = s.id,            
                name = s.name,
                age = s.age,
                gender = s.gender,
                district = s.district,  
                postcode = s.postcode,   
                occupation = s.occupation,
                eliminated = false
            });
        }
        return ds;
    }
}
