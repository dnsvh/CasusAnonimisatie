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

    // Timer die in de HUD wordt opgeteld
    public float roundTimer = 0f;

    // Tijden per ronde + laatst bekende totaaltijd (voor Debrief/Leaderboard)
    public float roundASeconds = 0f;
    public float roundBSeconds = 0f;
    public float lastTotalSeconds = 0f;

    public SuspectDataset datasetA;
    public SuspectDataset datasetB;

    [SerializeField] private string killerId = null;

    // Clue-variatie tracking
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

        if (datasetA == null || datasetA.suspects == null || datasetA.suspects.Count == 0)
            datasetA = GenerateSynthetic(30);

        if (datasetB == null || datasetB.suspects == null || datasetB.suspects.Count == 0)
            datasetB = CloneDataset(datasetA);

        if (string.IsNullOrEmpty(killerId) && datasetA.suspects.Count > 0)
            killerId = datasetA.suspects[UnityEngine.Random.Range(0, datasetA.suspects.Count)].id;

        if (!datasetB.suspects.Any(s => s.id == killerId) && datasetB.suspects.Count > 0)
            killerId = datasetB.suspects[0].id;

        Debug.Log($"[GM] Awake. Phase={phase}, killerId={killerId}");
    }

    public void TickTimer(float dt) => roundTimer += dt;

    public SuspectDataset CurrentDataset() => (phase == RoundPhase.RoundB) ? datasetB : datasetA;

    public IEnumerable<Suspect> RemainingSuspects()
        => CurrentDataset().suspects.Where(s => !s.eliminated);

    public Suspect GetKiller()
        => CurrentDataset().suspects.FirstOrDefault(s => s.id == killerId);

    void EnsureKillerSelected()
    {
        if (!string.IsNullOrEmpty(killerId)) return;

        var src = (datasetA != null && datasetA.suspects != null && datasetA.suspects.Count > 0)
            ? datasetA.suspects
            : datasetB?.suspects;

        if (src != null && src.Count > 0)
        {
            var pick = src[UnityEngine.Random.Range(0, src.Count)];
            killerId = pick.id;
            Debug.Log($"[GM] Killer fixed at {pick.name} ({killerId})");
        }
    }

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
        usedCluesA.Clear();
        foreach (var s in datasetA.suspects) s.eliminated = false;
    }

    public void BeginRoundB()
    {
        // Leg de eindtijd van A vast voordat we resetten
        roundASeconds = roundTimer;

        phase = RoundPhase.RoundB;
        roundTimer = 0f;
        usedCluesB.Clear();
        foreach (var s in datasetB.suspects) s.eliminated = false;

        Debug.Log($"[GM] BeginRoundB. Keeping killerId={killerId}");
    }

    // Accuse flow: correct in A -> naar B; incorrect in A -> Start.
    // In B -> Debrief (win/lose).
    public void Accuse(string suspectId)
    {
        EnsureKillerSelected();
        var killer = GetKiller();
        bool correct = (killer != null && suspectId == killer.id);

        Debug.Log($"[GM] Accuse called with suspectId={suspectId}");
        Debug.Log($"[GM] Killer is {killer?.name} ({killer?.id}) in phase {phase}");

        if (phase == RoundPhase.RoundA)
        {
            if (correct)
            {
                lastAccuseCorrect = true;
                Debug.Log("[GM] Accuse: CORRECT (Round A) -> Round B");
                BeginRoundB(); // note: zet roundASeconds
                SceneManager.LoadScene("TheCityAnon");
            }
            else
            {
                lastAccuseCorrect = false;
                // Voor volledigheid: totale tijd is alleen ronde A
                roundASeconds = roundTimer;
                lastTotalSeconds = roundASeconds;
                Debug.Log("[GM] Accuse: WRONG (Round A) -> Start");
                SceneManager.LoadScene("Start");
            }
        }
        else
        {
            // Leg eindtijd ronde B vast en totaliseer
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

    // --- Clue variety (AgeExact/AgeRange = één “age bucket”) ---
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

    // --- dataset helpers ---
    private SuspectDataset GenerateSynthetic(int count)
    {
        var rnd = new System.Random();
        var ds = new SuspectDataset();
        string[] genders = { "M", "F" };
        string[] districts = { "Noord", "Zuid", "Oost", "West", "Centrum" };
        string[] occupations = { "Bakker", "Leraar", "Student", "Arts", "Chauffeur", "Programmeur" };

        for (int i = 0; i < count; i++)
        {
            var s = new Suspect
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 8),
                name = $"Persoon{i + 1}",
                age = rnd.Next(18, 70),
                gender = genders[rnd.Next(genders.Length)],
                district = districts[rnd.Next(districts.Length)],
                postcode = $"{(char)('1' + rnd.Next(0, 8))}{(char)('0' + rnd.Next(0, 9))}{(char)('A' + rnd.Next(0, 26))}{(char)('A' + rnd.Next(0, 26))}",
                occupation = occupations[rnd.Next(occupations.Length)],
                eliminated = false
            };
            ds.suspects.Add(s);
        }
        return ds;
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
