using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class NPCInteractable : MonoBehaviour
{
    public float interactRadius = 1.2f;
    public NPCDialogue dialogue;
    private Transform player;

    void Awake()
    {
        if (dialogue == null) dialogue = GetComponent<NPCDialogue>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p ? p.transform : null;
    }

    void Update()
    {
        if (player == null || dialogue == null) return;

        float d = Vector2.Distance(player.position, transform.position);
        var kb = Keyboard.current;
        bool pressE = kb != null && kb.eKey.wasPressedThisFrame;

        if (d <= interactRadius && pressE)
            ShowDialogAndApplyClue();
    }

    void ShowDialogAndApplyClue()
    {
        if (GameManager.I == null) { Debug.LogWarning("[NPCInteractable] GameManager.I is null."); return; }

        var gm = GameManager.I;
        var killer = gm.GetKiller();
        if (killer == null) { Debug.LogWarning("[NPCInteractable] Killer is null."); return; }

        bool anonymized = gm.phase == RoundPhase.RoundB;

        var effectiveType = gm.ChooseClueType(dialogue.clueType);
        gm.MarkClueUsed(effectiveType);

        string line = (dialogue.introDutch ?? "Hoi!") + "\n" + dialogue.GetClueLine(killer, anonymized, effectiveType);
        DialogueController.I?.Show(line);

        ApplyFilter(gm, effectiveType, anonymized, killer);

        GameHUDController.I?.RefreshSuspectList();
        GameHUDController.I?.RefreshMetrics();
    }

    static string Post2(string pc) => Anonymizer.GeneralizePostcode(pc, 2);

    void ApplyFilter(GameManager gm, ClueType type, bool anonymized, Suspect k)
    {
        switch (type)
        {
            case ClueType.AgeExact:
                if (anonymized) gm.EliminateByClue(s => Anonymizer.AgeBin(s.age) == Anonymizer.AgeBin(k.age));
                else            gm.EliminateByClue(s => s.age == k.age);
                break;

            case ClueType.AgeRange:
                if (anonymized) gm.EliminateByClue(s => Anonymizer.AgeBin(s.age) == Anonymizer.AgeBin(k.age));
                else            gm.EliminateByClue(s => s.age == k.age);
                break;

            case ClueType.Gender:
                gm.EliminateByClue(s => s.gender == k.gender);
                break;

            case ClueType.District:
                if (anonymized) gm.EliminateByClue(s => s.district == k.district || Post2(s.postcode) == Post2(k.postcode));
                else            gm.EliminateByClue(s => s.district == k.district);
                break;

            case ClueType.Postcode2:
                if (anonymized) gm.EliminateByClue(s => Post2(s.postcode) == Post2(k.postcode));
                else            gm.EliminateByClue(s => s.postcode == k.postcode);
                break;

            case ClueType.Occupation:
                if (anonymized) gm.EliminateByClue(s => true); 
                else            gm.EliminateByClue(s => s.occupation == k.occupation);
                break;
        }
    }
}
