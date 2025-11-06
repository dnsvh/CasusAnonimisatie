using UnityEngine;

public class NPCClueAssigner : MonoBehaviour
{
    [Tooltip("Assign clues on Start() automatically")]
    public bool assignOnStart = true;

    void Start()
    {
        if (assignOnStart) AssignAll();
    }

    [ContextMenu("Assign clues to all NPCs in scene")]
    public void AssignAll()
    {
        var npcs = Object.FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        if (npcs == null || npcs.Length == 0) return;

        var types = new[]
        {
            ClueType.AgeExact,
            ClueType.AgeRange,
            ClueType.Gender,
            ClueType.District,
            ClueType.Postcode2,
            ClueType.Occupation
        };

        int i = 0;
        foreach (var n in npcs)
        {
            n.clueType = types[i % types.Length];
            i++;
        }

        Debug.Log($"[NPCClueAssigner] Assigned {npcs.Length} clues across {types.Length} types.");
    }
}
