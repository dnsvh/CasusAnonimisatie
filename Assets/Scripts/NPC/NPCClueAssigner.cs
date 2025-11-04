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
        // New API in Unity 6
        var npcs = Object.FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        if (npcs == null || npcs.Length == 0) return;

        // Cycle through a set of different clue types so NPCs don't repeat the same hint
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
