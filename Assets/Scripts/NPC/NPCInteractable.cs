using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour {
    public float interactRadius = 1.2f;
    private Transform player;
    private NPCDialogue dlg;

    private bool hasGivenClue = false;
    private string cachedLine = null;

    void Start() {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p ? p.transform : null;
        dlg = GetComponent<NPCDialogue>();
        if (player == null) Debug.LogWarning("[NPCInteractable] Player (tag=Player) niet gevonden.");
        if (dlg == null)    Debug.LogWarning("[NPCInteractable] NPCDialogue component ontbreekt.");
    }

    void Update() {
        if (player == null || dlg == null) return;

        float d = Vector2.Distance(player.position, transform.position);

        bool pressed = false;
        if (Keyboard.current != null) pressed |= Keyboard.current.eKey.wasPressedThisFrame;
        if (Gamepad.current  != null) pressed |= Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (d <= interactRadius && pressed) {
            ShowDialogAndApplyClue();
        }
    }

    void ShowDialogAndApplyClue() {
        if (hasGivenClue) {
            if (!string.IsNullOrEmpty(cachedLine)) DialogueController.I?.Show(cachedLine);
            return;
        }

        var gm = GameManager.I;
        if (gm == null) { DialogueController.I?.Show("Let op: gamedata is nog niet geladen."); return; }

        var killer = gm.GetKiller();
        if (killer == null) { DialogueController.I?.Show("Let op: dader is niet gekozen."); return; }

        bool anonymized = gm.phase == RoundPhase.RoundB;

        string intro = string.IsNullOrWhiteSpace(dlg.introDutch) ? RandomIntro() : dlg.introDutch;
        string clue  = dlg.GetClueLine(killer.age, killer.gender, killer.district, killer.postcode, killer.occupation, anonymized);
        string full  = intro + "\n" + clue;

        DialogueController.I?.Show(full);
        ApplyFilter(gm, dlg.clueType, anonymized,
                    killer.age, killer.gender, killer.district, killer.postcode, killer.occupation);

        hasGivenClue = true;
        cachedLine = full;

        var hud = Object.FindFirstObjectByType<GameHUDController>();
        hud?.RefreshSuspectList();
        hud?.RefreshMetrics();
    }

    void ApplyFilter(GameManager gm, ClueType type, bool anonymized,
                     int age, string gender, string district, string postcode, string occupation) {
        switch (type) {
            case ClueType.AgeExact:
                if (anonymized) gm.EliminateByClue(s => AgeBin(s.age) == AgeBin(age));
                else            gm.EliminateByClue(s => s.age == age);
                break;
            case ClueType.AgeRange:
                gm.EliminateByClue(s => AgeBin(s.age) == AgeBin(age));
                break;
            case ClueType.Gender:
                gm.EliminateByClue(s => s.gender == gender);
                break;
            case ClueType.District:
                if (anonymized) gm.EliminateByClue(s => s.district == district || s.postcode.StartsWith(SafePrefix(postcode,2)));
                else            gm.EliminateByClue(s => s.district == district);
                break;
            case ClueType.Postcode2:
                if (anonymized) gm.EliminateByClue(s => s.postcode.StartsWith(SafePrefix(postcode,2)));
                else            gm.EliminateByClue(s => s.postcode == postcode);
                break;
            case ClueType.Occupation:
                if (anonymized) gm.EliminateByClue(s => true);
                else            gm.EliminateByClue(s => s.occupation == occupation);
                break;
        }
    }

    public static string AgeBin(int age) {
        if (age < 25) return "18-24";
        if (age < 30) return "25-29";
        if (age < 40) return "30-39";
        if (age < 50) return "40-49";
        if (age < 60) return "50-59";
        return "60+";
    }
    string SafePrefix(string s, int n) => string.IsNullOrEmpty(s) || s.Length < n ? s : s.Substring(0, n);

    string RandomIntro() {
        string[] pool = {
            "Hoi, agent. Ik zag iets vreemds.",
            "Oh, een moordzaak? Ik kan je wel wat vertellen.",
            "Momentje… ik herinner me iets over die persoon.",
            "Ja, ik heb iemand gezien die verdacht leek.",
            "Ik was hier in de buurt en iets viel me op."
        };
        return pool[Random.Range(0, pool.Length)];
    }
}
