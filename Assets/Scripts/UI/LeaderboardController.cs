using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // for RectTransform anchoring helpers

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] TMP_Text header;
    [SerializeField] TMP_Text body;

    void Start()
    {
        // 1) Ensure we have UI labels (auto-find or auto-create so you don't need to drag)
        EnsureLabels();

        // 2) Get settings (ScriptableObject) without JSON
        var settings = (GameManager.I != null && GameManager.I.settings != null)
            ? GameManager.I.settings
            : Resources.Load<GameSettings>("Data/GameSettings"); // Asset path: Assets/Resources/Data/GameSettings.asset

        if (settings == null)
        {
            Debug.LogWarning("[Leaderboard] GameSettings not found at Resources/Data/GameSettings. Using fallbacks.");
        }

        // Title text
        var title = (settings != null && settings.ui.leaderboard != null) ? settings.ui.leaderboard : "Leaderboard";
        header.text = title;

        // 3) Load leaderboard entries
        var table = LeaderboardManager.Load(); // assume it never throws; handle nulls anyway
        var sb = new StringBuilder();
        if (table != null && table.scores != null && table.scores.Count > 0)
        {
            int i = 1;
            foreach (var s in table.scores)
            {
                // defend against null/empty names
                var name = string.IsNullOrWhiteSpace(s.name) ? "—" : s.name;
                sb.AppendLine($"{i,2}. {name} — {s.time:F1}s");
                i++;
            }
        }

        body.text = sb.Length > 0
            ? sb.ToString()
            : "Nog geen tijden.";
    }

    public void OnBack()
    {
        // If you have a SceneNames helper, keep it; else fall back to literal.
#if UNITY_EDITOR
        try { SceneManager.LoadScene(SceneNames.Start); }
        catch { SceneManager.LoadScene("Start"); }
#else
        try { SceneManager.LoadScene(SceneNames.Start); }
        catch { SceneManager.LoadScene("Start"); }
#endif
    }

    // ---------- helpers ----------

    void EnsureLabels()
    {
        // Try to find by common names if not assigned
        if (header == null)
        {
            header = TryFindText("Title") ?? TryFindText("Header");
        }
        if (body == null)
        {
            body = TryFindText("Explanation") ?? TryFindText("Body");
        }

        // If still missing, create simple labels under the first Canvas we find
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        if (header == null)
        {
            header = CreateLabel(canvas.transform, "Header",
                                 anchorMin: new Vector2(0.5f, 1f),
                                 anchorMax: new Vector2(0.5f, 1f),
                                 anchoredPos: new Vector2(0f, -80f),
                                 sizeDelta: new Vector2(900f, 80f),
                                 fontSize: 40,
                                 align: TextAlignmentOptions.Center);
        }

        if (body == null)
        {
            body = CreateLabel(canvas.transform, "Body",
                               anchorMin: new Vector2(0.5f, 0.5f),
                               anchorMax: new Vector2(0.5f, 0.5f),
                               anchoredPos: new Vector2(0f, -40f),
                               sizeDelta: new Vector2(900f, 600f),
                               fontSize: 28,
                               align: TextAlignmentOptions.TopLeft);
        }
    }

    TMP_Text TryFindText(string name)
    {
        var go = GameObject.Find(name);
        return go ? go.GetComponent<TMP_Text>() : null;
    }

    TMP_Text CreateLabel(Transform parent, string name,
                         Vector2 anchorMin, Vector2 anchorMax,
                         Vector2 anchoredPos, Vector2 sizeDelta,
                         int fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.enableWordWrapping = true;
        tmp.text = ""; // caller sets real text
        return tmp;
    }
}
