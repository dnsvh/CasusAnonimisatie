using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("State")]
    public RoundPhase phase = RoundPhase.RoundA;
    public float roundTimer = 0f;
    public string killerId;

    [Header("Data")]
    public SuspectDataset datasetA;   // optional source for Round A
    public SuspectDataset datasetB;   // generated for Round B
    public GameSettings settings;

    // Working set the HUD/NPCs read
    readonly List<Suspect> _work = new List<Suspect>();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (settings == null)
        {
            settings = Resources.Load<GameSettings>("Data/GameSettings");
            if (settings == null) settings = ScriptableObject.CreateInstance<GameSettings>();
        }

        // If no datasetA provided, create a tiny fallback so you can play
        if (datasetA == null)
        {
            datasetA = new SuspectDataset();
            for (int i=0; i<8; i++)
            {
                datasetA.suspects.Add(new Suspect {
                    id = System.Guid.NewGuid().ToString("N"),
                    name = "Persoon " + (i+1),
                    age = 22 + i,
                    gender = (i%2==0 ? "M" : "V"),
                    district = "Centrum",
                    postcode = "101" + i,
                    occupation = "Beroep " + (i%3),
                    eliminated = false
                });
            }
            Debug.Log("[GameManager] Using fallback datasetA");
        }

        // Build Round A working set
        _work.Clear();
        _work.AddRange(datasetA.suspects.Select(CloneSuspect));

        // Pick killer if missing
        if (string.IsNullOrEmpty(killerId) && _work.Count > 0)
            killerId = _work[Random.Range(0, _work.Count)].id;
    }

    Suspect CloneSuspect(Suspect s) => new Suspect {
        id = s.id, name = s.name, age = s.age, gender = s.gender,
        district = s.district, postcode = s.postcode, occupation = s.occupation, eliminated = s.eliminated
    };

    public IEnumerable<Suspect> RemainingSuspects() => _work.Where(s => !s.eliminated);

    public void EliminateByClue(System.Func<Suspect, bool> keep)
    {
        foreach (var s in _work) if (!keep(s)) s.eliminated = true;
    }

    public Suspect GetKiller() => _work.FirstOrDefault(s => s.id == killerId);

    public void TickTimer(float dt) => roundTimer += dt;

    public void Accuse(string suspectId)
    {
        if (string.IsNullOrEmpty(suspectId)) return;
        bool correct = (suspectId == killerId);
        Debug.Log($"[GameManager] Accuse {suspectId}. Correct={correct}. Phase={phase}");

        if (phase == RoundPhase.RoundA)
        {
            if (correct)
            {
                // Build Round B dataset if needed and generalize
                if (datasetB == null)
                {
                    datasetB = Anonymizer.Generalize(new SuspectDataset {
                        suspects = datasetA.suspects.Select(CloneSuspect).ToList()
                    });
                }

                _work.Clear();
                _work.AddRange(datasetB.suspects.Select(CloneSuspect));
                foreach (var s in _work) s.eliminated = false;
                phase = RoundPhase.RoundB;

                SceneManager.LoadScene("TheCityAnon");
                DialogueController.I?.Show("Goed gedaan! Ronde B is geanonimiseerd (categorieën & postcode-prefix).");
            }
            else
            {
                DialogueController.I?.Show("Helaas, verkeerde verdachte. Terug naar Start.");
                SceneManager.LoadScene("Start");
            }
        }
        else // RoundB → Debrief
        {
            DebriefState.LastWin = correct;
            SceneManager.LoadScene("Debrief");
        }
    }
}


