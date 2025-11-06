using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] TMP_Text header;
    [SerializeField] TMP_Text body;

    void Start()
    {
        EnsureLabels();

        var settings = (GameManager.I != null && GameManager.I.settings != null)
            ? GameManager.I.settings
            : Resources.Load<GameSettings>("Data/GameSettings"); 

        if (settings == null)
        {
            Debug.LogWarning("[Leaderboard] GameSettings not found at Resources/Data/GameSettings. Using fallbacks.");
        }


        var title = (settings != null && settings.ui.leaderboard != null) ? settings.ui.leaderboard : "Leaderboard";
        header.text = title;


        var table = LeaderboardManager.Load(); 
        var sb = new StringBuilder();
        if (table != null && table.scores != null && table.scores.Count > 0)
        {
            int i = 1;
            foreach (var s in table.scores)
            {

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

#if UNITY_EDITOR
        try { SceneManager.LoadScene(SceneNames.Start); }
        catch { SceneManager.LoadScene("Start"); }
#else
        try { SceneManager.LoadScene(SceneNames.Start); }
        catch { SceneManager.LoadScene("Start"); }
#endif
    }


    void EnsureLabels()
    {

        if (header == null)
        {
            header = TryFindText("Title") ?? TryFindText("Header");
        }
        if (body == null)
        {
            body = TryFindText("Explanation") ?? TryFindText("Body");
        }

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
        tmp.text = ""; 
        return tmp;
    }
}
