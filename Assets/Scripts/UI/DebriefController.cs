using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DebriefController : MonoBehaviour
{
    [Header("Auto build if missing")]
    public bool autoBuildUI = true;

    [Header("TMP references (auto-wired / auto-created)")]
    [SerializeField] TextMeshProUGUI titleLabel;
    [SerializeField] TextMeshProUGUI explanationText;
    [SerializeField] TextMeshProUGUI resultText;

    [Header("Optional inputs/buttons")]
    [SerializeField] TMP_InputField nicknameInput;
    [SerializeField] Button saveButton;
    [SerializeField] Button playAgainButton;

    void Awake()
    {
        AutoWire();
        if (autoBuildUI) EnsureUI();
        AutoWire(); // wire again in case we just created
    }

    void Start()
    {
        var gm = GameManager.I;
        bool won = gm && gm.lastAccuseCorrect;

        if (titleLabel) titleLabel.text = "Resultaat";
        if (resultText) resultText.text = won ? "Gefeliciteerd! 🎉 Je keuze was correct."
                                                   : "Helaas! ❌ Je keuze was niet correct.";

        if (explanationText)
        {
            string killer = gm?.GetKiller()?.name ?? "onbekend";
            int leftA = gm?.datasetA?.suspects?.Count(s => !s.eliminated) ?? 0;
            int leftB = gm?.datasetB?.suspects?.Count(s => !s.eliminated) ?? 0;
            explanationText.text =
                $"Dader: {killer}\n" +
                $"Ronde A resterend: {leftA}\n" +
                $"Ronde B resterend: {leftB}\n" +
                $"Tip: in ronde B worden hints gegeneraliseerd (leeftijdsbins, postcode-inkorting, etc.).";
        }

        if (saveButton) saveButton.onClick.AddListener(OnSaveNickname);
        if (playAgainButton) playAgainButton.onClick.AddListener(PlayAgain);
    }

    public void OnSaveNickname()
    {
        var name = nicknameInput ? nicknameInput.text : "";
        if (!string.IsNullOrWhiteSpace(name)) PlayerPrefs.SetString("nickname", name);
        Debug.Log($"[Debrief] Nickname saved: {name}");
    }

    void PlayAgain()
    {
        if (GameManager.I) GameManager.I.ResetToRoundA();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }

    // ---------- helpers ----------
    void AutoWire()
    {
        if (!titleLabel) titleLabel = FindByContains<TextMeshProUGUI>("title", "result");
        if (!explanationText) explanationText = FindByContains<TextMeshProUGUI>("uitleg", "explanation", "info");
        if (!resultText) resultText = FindByContains<TextMeshProUGUI>("result", "uitkomst", "status");

        if (!nicknameInput) nicknameInput = FindByContains<TMP_InputField>("nickname", "naam", "input");
        if (!saveButton) saveButton = FindByContains<Button>("save", "opslaan");
        if (!playAgainButton) playAgainButton = FindByContains<Button>("again", "retry", "opnieuw", "start");
    }

    T FindByContains<T>(params string[] tokens) where T : Component
    {
        var all = GameObject.FindObjectsOfType<T>(includeInactive: true);
        foreach (var c in all)
        {
            string n = c.gameObject.name.ToLowerInvariant();
            if (tokens.Any(t => n.Contains(t))) return c;
        }
        return null;
    }

    void EnsureUI()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (!canvas)
        {
            var go = new GameObject("DebriefCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Create a parent panel to keep things tidy
        var root = GameObject.Find("DebriefUI") ?? new GameObject("DebriefUI", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        var rtRoot = root.GetComponent<RectTransform>();
        rtRoot.anchorMin = new Vector2(0, 0);
        rtRoot.anchorMax = new Vector2(1, 1);
        rtRoot.offsetMin = rtRoot.offsetMax = Vector2.zero;

        if (!titleLabel) titleLabel = CreateTMP(rtRoot, "Title", new Vector2(0.5f, 0.9f), 64, TextAlignmentOptions.Center);
        if (!explanationText) explanationText = CreateTMP(rtRoot, "Explanation", new Vector2(0.15f, 0.8f), 36, TextAlignmentOptions.TopLeft, new Vector2(600, 400));
        if (!resultText) resultText = CreateTMP(rtRoot, "Result", new Vector2(0.75f, 0.5f), 48, TextAlignmentOptions.Center, new Vector2(700, 250));
    }

    TextMeshProUGUI CreateTMP(RectTransform parent, string name, Vector2 anchor, int size,
                               TextAlignmentOptions align, Vector2? box = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = box ?? new Vector2(800, 120);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.enableWordWrapping = true;
        tmp.text = ""; // will be filled in Start
        return tmp;
    }
}
