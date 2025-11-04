using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour {
    public static DialogueController I;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text textField;

    void Awake() {
        I = this;
        Hide();
    }

    public void Show(string text) {
        if (textField != null) textField.text = text;
        if (group != null) {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }
        CancelInvoke();
        Invoke(nameof(Hide), 4f); // auto-hide after 4 seconds
    }

    public void Hide() {
        if (group != null) {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}
