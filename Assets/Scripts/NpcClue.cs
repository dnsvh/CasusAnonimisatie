using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class NpcClue : MonoBehaviour
{
    [Header("Clue to give (RAW)")]
    public ClueField field = ClueField.Age;
    public string value = "34";
    public string npcLineIfEasy = "I think they were 34!";
    public string npcLineIfHard = "Looked somewhere in the 30–40 range.";

    [Header("UI Prompt")]
    public GameObject pressEIndicator;   // optional
    public TextMeshProUGUI speechBubble; // optional

    private bool playerIn;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIn = true;
            if (pressEIndicator) pressEIndicator.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIn = false;
            if (pressEIndicator) pressEIndicator.SetActive(false);
            if (speechBubble) speechBubble.text = "";
        }
    }

    private bool PressedInteract()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && k.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private void Update()
    {
        if (!playerIn) return;
        if (!PressedInteract()) return;

        // say the line
        string line = (GameController.I != null && GameController.I.mode == GameMode.Easy)
            ? npcLineIfEasy
            : npcLineIfHard;
        if (speechBubble) speechBubble.text = line;

        // send the raw clue
        var raw = new RawClue { field = field, op = "eq", value = value };
        GameController.I.OnClueReceived(raw);
    }
}
