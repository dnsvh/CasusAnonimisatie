using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public GameSettings settings;                  // Optional: ScriptableObject in Resources/Data/GameSettings
    public RoundPhase phase = RoundPhase.RoundA;
    public float roundTimer = 0f;

    public SuspectDataset datasetA;                // Round A dataset (precise)
    public SuspectDataset datasetB;                // Round B dataset (same ids; anonymized presentation)

    [SerializeField] private string killerId = null;   // Persist SAME killer across rounds

    // Track used clue types per round (for variety)
    private readonly HashSet<ClueType> usedCluesA = new HashSet<ClueType>();
    private readonly HashSet<ClueType> usedCluesB = new HashSet<ClueType>();

    // Debrief needs to know last result
    public bool lastAccuseCorrect = false;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // Try to load settings from Resources (optional)
        if (settings == null)
        {
            settings = Resources.Load<GameSettings>("Data/GameSettings");
        }

        // Ensure we have datasets
        if (datasetA == null || datasetA.suspects == null || datasetA.suspects.Count == 0)
            datasetA = GenerateSynthetic(30);

        if (datasetB == null || datasetB.suspects == null || datasetB.suspects.Count == 0)
            datasetB = CloneDataset(datasetA);

        // Pick a killer if not set yet (from A)
        if (string.IsNullOrEmpty(killerId) && datasetA.suspects.Count > 0)
            killerId = datasetA.suspects[UnityEngine.Random.Range(0, datasetA.suspects.Count)].id;

        // Ensure B also contains that id (fallback if not)
        if (!datasetB.suspects.Any(s => s.id == killerId) && datasetB.suspects.Count > 0)
            killerId = datasetB.suspects[0].id;

        Debug.Log($"[GM] Awake. Phase={phase}, killerId={killerId}");
    }

    // Called from HUD update
    public void TickTimer(float dt) => roundTimer += dt;

    public SuspectDataset CurrentDataset() => (phase == RoundPhase.RoundB) ? datasetB : datasetA;

    public IEnumerable<Suspect> RemainingSuspects()
    {
        var data = CurrentDataset();
        return data.suspects.Where(s => !s.eliminated);
    }

    public Suspect GetKiller()
    {
        // Always look in the CURRENT round dataset by id
        var data = CurrentDataset();
        return data.suspects.FirstOrDefault(s => s.id == killerId);
    }

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

    // Keep only suspects that match the predicate; others get eliminated=true
    public void EliminateByClue(Func<Suspect, bool> keepPredicate)
    {
        var data = CurrentDataset();
        foreach (var s in data.suspects)
        {
            if (!keepPredicate(s))
                s.eliminated = true;
        }
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
        usedCluesA.Clear();
        foreach (var s in datasetA.suspects) s.eliminated = false;
        Debug.Log("[GM] ResetToRoundA");
    }

    public void BeginRoundB()
    {
        phase = RoundPhase.RoundB;
        roundTimer = 0f;
        usedCluesB.Clear();
        foreach (var s in datasetB.suspects) s.eliminated = false;
        Debug.Log($"[GM] BeginRoundB. Keeping killerId={killerId}");
    }

    // Accuse flow: correct in A -> go to B; incorrect in A -> restart.
    // In B -> go to Debrief with result.
    public void Accuse(string suspectId)
    {
        EnsureKillerSelected();
        var killer = GetKiller();
        bool correct = (killer != null && suspectId == killer.id);

        Debug.Log($"[GM] Accuse called with suspectId={suspectId}");
        Debug.Log($"[GM] Killer is {killer?.name} ({killer?.id}) in phase {phase}");

        // write result to both a field and PlayerPrefs (fallback for Debrief)
        lastAccuseCorrect = correct;
        PlayerPrefs.SetInt("LastWon", correct ? 1 : 0);
        PlayerPrefs.SetString("LastKillerId", killer?.id ?? "");
        PlayerPrefs.SetString("LastKillerName", killer?.name ?? "");
        PlayerPrefs.Save();

        if (phase == RoundPhase.RoundA)
        {
            if (correct)
            {
                Debug.Log("[GM] Accuse: CORRECT (Round A) -> Round B");
                BeginRoundB(); // keeps killerId
                SceneManager.LoadScene("TheCityAnon");
            }
            else
            {
                Debug.Log("[GM] Accuse: WRONG (Round A) -> Start");
                SceneManager.LoadScene("Start");
            }
        }
        else // Round B
        {
            if (correct)
                Debug.Log("[GM] Accuse (Round B): CORRECT. To Debrief.");
            else
                Debug.Log("[GM] Accuse (Round B): WRONG. To Debrief.");

            SceneManager.LoadScene("Debrief");
        }
    }


    // --- Clue variety: treat AgeExact/AgeRange as the same "age bucket" ---

    public ClueType ChooseClueType(ClueType preferred)
    {
        var used = (phase == RoundPhase.RoundB) ? usedCluesB : usedCluesA;

        bool Age(ClueType t) => t == ClueType.AgeExact || t == ClueType.AgeRange;
        bool anyAgeUsed = used.Contains(ClueType.AgeExact) || used.Contains(ClueType.AgeRange);

        // If preferred is unused and (not age OR no age used yet), take it.
        if (!used.Contains(preferred) && (!Age(preferred) || !anyAgeUsed))
            return preferred;

        // Try to find any non-age unused type first (for variety)
        var all = new ClueType[] {
            ClueType.Gender, ClueType.District, ClueType.Postcode2, ClueType.Occupation,
            ClueType.AgeExact, ClueType.AgeRange
        };
        foreach (var t in all)
            if (!used.Contains(t) && (!Age(t) || !anyAgeUsed))
                return t;

        // If everything non-age is used, allow one age (if not both already used)
        foreach (var t in new ClueType[] { ClueType.AgeExact, ClueType.AgeRange })
            if (!used.Contains(t))
                return t;

        // All used: allow repeat of the preferred
        return preferred;
    }

    public void MarkClueUsed(ClueType t)
    {
        var used = (phase == RoundPhase.RoundB) ? usedCluesB : usedCluesA;

        // Age bucket: using one blocks the other
        if (t == ClueType.AgeExact || t == ClueType.AgeRange)
        {
            used.Add(ClueType.AgeExact);
            used.Add(ClueType.AgeRange);
            return;
        }

        used.Add(t);
    }

    // --- Helpers to create data if none exists ---

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
                id = s.id,               // IMPORTANT: preserve id across rounds
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
