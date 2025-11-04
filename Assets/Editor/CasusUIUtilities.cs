#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CasusUIUtilities
{
    [MenuItem("Tools/CasusAnonimisatie/SMOKE: UI utilities alive")]
    public static void Smoke() {
        Debug.Log("Casus UI utilities menu is actief ✅");
    }

    [MenuItem("Tools/CasusAnonimisatie/Style Dialogue Bar in OPEN scene")]
    public static void StyleDialogueBar()
    {
        // Try by name first
        GameObject bar = GameObject.Find("DialogueBar");

        // If not found, try to locate by DialogueController
        if (bar == null)
        {
            var dlg = Object.FindFirstObjectByType<DialogueController>(FindObjectsInactive.Include);
            if (dlg != null) bar = dlg.gameObject;
        }

        if (bar == null)
        {
            Debug.LogWarning("DialogueBar niet gevonden (naam of DialogueController). Open TheCity en zorg dat de HUD/Dialoog bestaat.");
            return;
        }

        // Ensure background
        var img = bar.GetComponent<Image>();
        if (img == null) img = bar.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f); // dark translucent

        // Find the text child called "Text" (what our auto-wiring used), or any TMP in children
        TMP_Text tmp = null;
        var t = bar.transform.Find("Text");
        if (t != null) tmp = t.GetComponent<TMP_Text>();
        if (tmp == null) tmp = bar.GetComponentInChildren<TMP_Text>();

        if (tmp != null)
        {
            tmp.color = Color.white;
            if (tmp.fontSize < 20) tmp.fontSize = 22f;
            tmp.enableWordWrapping = true;
            tmp.alignment = TextAlignmentOptions.TopLeft;
        }
        else
        {
            Debug.LogWarning("Kon geen TMP_Text in DialogueBar vinden. Check of er een TMP tekstcomponent als child is.");
        }

        // Make sure it’s visible
        var cg = bar.GetComponent<CanvasGroup>();
        if (cg == null) cg = bar.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        Debug.Log("DialogueBar gestyled (donkere achtergrond + witte, leesbare tekst).");
    }
}
#endif
