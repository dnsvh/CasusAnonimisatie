using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebriefController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI killerText;
    [SerializeField] TextMeshProUGUI totalTimeText;

    [SerializeField] TextMeshProUGUI leaderboardHeader;
    [SerializeField] TextMeshProUGUI leaderboardBody;

    [SerializeField] RectTransform entryRow;
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] TMP_InputField nicknameInput;
    [SerializeField] Button saveButton;

    [SerializeField] Button backButton;
    [SerializeField] TextMeshProUGUI explanationText;

    const int TopN = 10;

    void Start()
    {
        EnsureLayout();
        RebuildEntryRow();        // fresh, always-on-top row with proper input
        RefreshUI();
        HideLegacyDuplicates();
    }

    // ---------------- Layout ----------------
    void EnsureLayout()
    {
        var rootCanvas = FindFirstObjectByType<Canvas>();
        if (!rootCanvas)
        {
            var cGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootCanvas = cGo.GetComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var cs = cGo.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.matchWidthOrHeight = 0.5f;
        }

        // Title center-top
        if (!titleText) titleText = MakeText("Title", rootCanvas.transform,
            new(0.5f, 1), new(0.5f, 1), new(0.5f, 1),
            new(0, -70), Vector2.zero, 64, TextAlignmentOptions.Center, true);

        if (!killerText) killerText = MakeText("Killer", rootCanvas.transform,
            new(0.5f, 1), new(0.5f, 1), new(0.5f, 1),
            new(0, -140), Vector2.zero, 38, TextAlignmentOptions.Center, true);

        // Right block
        var rightPanel = EnsurePanel("RightPanel", rootCanvas.transform,
            new(1, 1), new(1, 1), new(1, 1),
            new(-24, -24), new(520, 560), 0f, false);

        if (!totalTimeText) totalTimeText = MakeText("TotalTime", rightPanel,
            new(1, 1), new(1, 1), new(1, 1),
            new(-8, -8), Vector2.zero, 30, TextAlignmentOptions.TopRight, true);

        if (!leaderboardHeader) leaderboardHeader = MakeText("LeaderboardHeader", rightPanel,
            new(1, 1), new(1, 1), new(1, 1),
            new(-8, -52), Vector2.zero, 26, TextAlignmentOptions.TopRight, true);

        var scroll = EnsureScroll("LeaderboardScroll", rightPanel, new(-8, -88), new(500, 360), 0.10f);

        if (!leaderboardBody)
        {
            leaderboardBody = MakeText("LeaderboardBody", scroll.content,
                new(0, 1), new(1, 1), new(0, 1),
                Vector2.zero, Vector2.zero, 24, TextAlignmentOptions.TopRight, true);
            var rt = leaderboardBody.rectTransform;
            rt.offsetMin = new Vector2(12, -360);
            rt.offsetMax = new Vector2(-4, 0);
        }

        // Bottom explanation
        var bottomPanel = EnsurePanel("BottomExplanation", rootCanvas.transform,
            new(0.5f, 0), new(0.5f, 0), new(0.5f, 0),
            new(0, 160), new(1300, 230), 0f, false);

        if (!explanationText)
        {
            explanationText = MakeText("Explanation", bottomPanel,
                new(0, 0), new(1, 1), new(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 22, TextAlignmentOptions.Center, true);
            explanationText.textWrappingMode = TextWrappingModes.Normal;
        }

        if (!backButton)
        {
            backButton = MakeButton("Terug", rootCanvas.transform,
                new(24, 24), new(130, 42),
                new(0, 0), new(0, 0), new(0, 0));
            backButton.onClick.AddListener(OnBack);
        }

        if (!FindFirstObjectByType<EventSystem>())
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    void RebuildEntryRow()
    {
        var old = GameObject.Find("EntryRow");
        if (old) Destroy(old);

        var rightPanel = GameObject.Find("RightPanel")?.GetComponent<RectTransform>()
                         ?? FindFirstObjectByType<Canvas>().GetComponent<RectTransform>();

        var rowGO = new GameObject("EntryRow",
            typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        rowGO.transform.SetParent(rightPanel, false);
        entryRow = rowGO.GetComponent<RectTransform>();
        entryRow.anchorMin = new Vector2(1, 1);
        entryRow.anchorMax = new Vector2(1, 1);
        entryRow.pivot = new Vector2(1, 0.5f);
        entryRow.anchoredPosition = new Vector2(-8, -(88 + 360 + 16));
        entryRow.sizeDelta = new Vector2(500, 50);

        var subCanvas = rowGO.GetComponent<Canvas>();
        subCanvas.overrideSorting = true;
        subCanvas.sortingOrder = 5000;   // sits above other UI

        // Label
        nameLabel = MakeText("NaamLabel", entryRow,
            new(0, 0.5f), new(0, 0.5f), new(0, 0.5f),
            new(6, 0), new(90, 48), 22, TextAlignmentOptions.MidlineLeft, false);
        nameLabel.text = "Naam:";

        // Save button (120 px)
        saveButton = MakeButton("Opslaan", entryRow,
            new(-2, 0), new(120, 48),
            new(1, 0.5f), new(1, 0.5f), new(1, 0.5f));
        saveButton.transform.SetAsLastSibling();

        // Input (280 px) – TMP standard structure using RectMask2D
        nicknameInput = MakeTMPInput(
            "Nickname", entryRow,
            new(0, 0.5f), new(0, 0.5f), new(0, 0.5f),
            new(100, 0), new(280, 48));

        // make sure the button is not covered
        nicknameInput.GetComponent<RectTransform>().SetSiblingIndex(saveButton.transform.GetSiblingIndex() - 1);
    }

    // ---------------- Content ----------------
    void RefreshUI()
    {
        var gm = GameManager.I;

        SetText(titleText, gm && gm.lastAccuseCorrect ? "Gefeliciteerd!" : "Lastig hè?");
        SetText(killerText, $"De dader was: {gm?.GetKiller()?.name ?? "onbekend"}");

        float total = gm && gm.lastTotalSeconds > 0f
            ? gm.lastTotalSeconds
            : (gm ? Mathf.Max(0, gm.roundASeconds) + Mathf.Max(0, gm.roundBSeconds) : 0f);

        SetText(totalTimeText, $"Totale tijd: {Format(total)}");

        var table = LeaderboardManager.Load();
        SetText(leaderboardHeader, "Leaderboard (Top 10)");
        SetText(leaderboardBody, BuildLeaderboard(table), wrap: true);

        bool underCap = table == null || table.scores == null || table.scores.Count < TopN;
        float worst = (table != null && table.scores != null && table.scores.Count > 0)
                        ? table.scores.Max(s => s.time)
                        : float.MaxValue;
        bool mayEnter = underCap || total <= worst + 0.0001f;
        bool canSubmit = (gm && gm.lastAccuseCorrect) && mayEnter;

        Toggle(entryRow, canSubmit);

        if (canSubmit && nicknameInput)
        {
            if (nicknameInput.placeholder is TMP_Text ph) ph.text = "Naam (max 24 tekens)";
            nicknameInput.text = "";
            EventSystem.current?.SetSelectedGameObject(nicknameInput.gameObject);
            nicknameInput.Select();
            nicknameInput.ActivateInputField();
        }

        if (saveButton)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(() => OnSaveNickname(total));
        }

        SetText(explanationText,
            "Uitleg\n" +
            "• Je speelde twee rondes: Round A (precies) en Round B (geanonimiseerd).\n" +
            "• Totale tijd = tijd van beide rondes samen.\n" +
            "• Win je en sta je bij de snelste 10? Vul je naam in en sla je tijd op.\n" +
            "• Hoger k/l = meer privacy → zoeken wordt lastiger.\n" +
            "• Vergelijk je gevoel tussen Round A en B: hoe beïnvloedt anonimisering je strategie?\n",
            wrap: true);
    }

    public void OnSaveNickname()
    {
        var gm = GameManager.I;
        float total = (gm && gm.lastTotalSeconds > 0f)
            ? gm.lastTotalSeconds
            : (gm ? Mathf.Max(0f, gm.roundASeconds) + Mathf.Max(0f, gm.roundBSeconds) : 0f);
        OnSaveNickname(total);
    }

    public void OnSaveNickname(float total)
    {
        string nick = nicknameInput ? nicknameInput.text?.Trim() : null;
        if (string.IsNullOrEmpty(nick)) nick = "Anoniem";
        if (nick.Length > 24) nick = nick.Substring(0, 24);

        LeaderboardManager.AddScore(nick, total, TopN);

        SetText(leaderboardBody, BuildLeaderboard(LeaderboardManager.Load()), wrap: true);
        Toggle(entryRow, false);
    }

    public void OnBack() => SceneManager.LoadScene("Start");

    // ---------------- Helpers ----------------
    static string BuildLeaderboard(ScoreTable t)
    {
        if (t == null || t.scores == null || t.scores.Count == 0) return "Nog geen tijden.";
        var sb = new StringBuilder();
        int i = 1;
        foreach (var s in t.scores.OrderBy(x => x.time).Take(TopN))
        {
            sb.AppendLine($"{i,2}. {s.name} — {Format(s.time)}");
            i++;
        }
        return sb.ToString();
    }

    static string Format(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        float s = seconds - m * 60f;
        return $"{m}:{s:00.0}";
    }

    void SetText(TMP_Text t, string value, bool wrap = false)
    {
        if (!t) return;
        t.text = value;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
    }

    void Toggle(Behaviour b, bool on) { if (b) b.gameObject.SetActive(on); }
    void Toggle(RectTransform rt, bool on) { if (rt) rt.gameObject.SetActive(on); }

    void HideLegacyDuplicates()
    {
        foreach (var txt in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (txt == titleText || txt == killerText || txt == totalTimeText ||
                txt == leaderboardHeader || txt == leaderboardBody || txt == explanationText ||
                (nicknameInput && txt == nicknameInput.textComponent) ||
                (nicknameInput && txt == nicknameInput.placeholder)) continue;

            var s = txt.text?.Trim().ToLowerInvariant();
            if (s == "resultaat" || s == "uitleg..." || s == "uitleg")
                txt.gameObject.SetActive(false);
        }
    }

    // ---------- UI builders ----------
    RectTransform EnsurePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchored, Vector2 size, float bgAlpha, bool raycast)
    {
        var go = GameObject.Find(name) ?? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (go.transform.parent != parent) go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchored; rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = new Color(1, 1, 1, bgAlpha);
        img.raycastTarget = raycast;
        return rt;
    }

    TextMeshProUGUI MakeText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchored, Vector2 size, int fontSize,
        TextAlignmentOptions align, bool autosize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchored; rt.sizeDelta = size;

        var t = go.GetComponent<TextMeshProUGUI>();
        t.fontSize = fontSize; t.alignment = align; t.color = Color.white;
        t.enableAutoSizing = autosize;
        t.fontSizeMax = fontSize; t.fontSizeMin = Mathf.Max(16, fontSize - 20);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Button MakeButton(string label, Transform parent, Vector2 anchored, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchored; rt.sizeDelta = size;

        var img = go.GetComponent<Image>(); img.color = new Color(1, 1, 1, 0.25f);
        img.raycastTarget = true;

        var txt = MakeText("Label", go.transform,
            new(0.5f, 0.5f), new(0.5f, 0.5f), new(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 22,
            TextAlignmentOptions.Center, true);
        txt.text = label; txt.color = Color.white;

        return go.GetComponent<Button>();
    }

    ScrollRect EnsureScroll(string name, Transform parent, Vector2 topRight, Vector2 size, float bgAlpha)
    {
        var go = GameObject.Find(name);
        if (!go)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            go.transform.SetParent(parent, false);
        }
        var img = go.GetComponent<Image>(); img.color = new Color(1, 1, 1, bgAlpha);
        img.raycastTarget = true;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = topRight; rt.sizeDelta = size;

        var sr = go.GetComponent<ScrollRect>();
        sr.horizontal = false;

        if (!sr.content)
        {
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(go.transform, false);
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0, 1);
            crt.offsetMin = new Vector2(8, -320);
            crt.offsetMax = new Vector2(-8, 0);
            sr.content = crt;
        }
        return sr;
    }

    // NEW: reliable TMP input using RectMask2D (matches TMP template)
    TMP_InputField MakeTMPInput(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchored, Vector2 size)
    {
        // Root
        var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        root.transform.SetParent(parent, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchored; rt.sizeDelta = size;

        var bg = root.GetComponent<Image>();
        bg.color = new Color(0.97f, 0.97f, 0.97f, 1f); // light gray so the text is obvious
        bg.raycastTarget = true;

        // Text Area (matches TMP default)
        var area = new GameObject("Text Area", typeof(RectTransform));
        area.transform.SetParent(root.transform, false);
        var areaRT = area.GetComponent<RectTransform>();
        areaRT.anchorMin = new Vector2(0, 0);
        areaRT.anchorMax = new Vector2(1, 1);
        areaRT.pivot = new Vector2(0.5f, 0.5f);
        areaRT.offsetMin = new Vector2(6, 6);
        areaRT.offsetMax = new Vector2(-6, -6);

        // Viewport with RectMask2D (this is key)
        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        vpGO.transform.SetParent(area.transform, false);
        var vpRT = vpGO.GetComponent<RectTransform>();
        vpRT.anchorMin = new Vector2(0, 0);
        vpRT.anchorMax = new Vector2(1, 1);
        vpRT.pivot = new Vector2(0, 1);
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;

        // Text
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(vpGO.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.pivot = new Vector2(0, 1);
        textRT.offsetMin = new Vector2(0, 0);
        textRT.offsetMax = new Vector2(0, 0);

        var text = textGO.GetComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 28;
        text.color = Color.black;                // live text visible
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.margin = new Vector4(4, 0, 4, 0);
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;

        // Placeholder
        var phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phGO.transform.SetParent(vpGO.transform, false);
        var phRT = phGO.GetComponent<RectTransform>();
        phRT.anchorMin = new Vector2(0, 0);
        phRT.anchorMax = new Vector2(1, 1);
        phRT.pivot = new Vector2(0, 1);
        phRT.offsetMin = new Vector2(0, 0);
        phRT.offsetMax = new Vector2(0, 0);

        var ph = phGO.GetComponent<TextMeshProUGUI>();
        ph.text = "Naam (max 24 tekens)";
        ph.fontSize = 24;
        ph.color = new Color(0f, 0f, 0f, 0.45f);
        ph.alignment = TextAlignmentOptions.MidlineLeft;
        ph.raycastTarget = false;
        ph.textWrappingMode = TextWrappingModes.NoWrap;

        // Wire the input
        var input = root.GetComponent<TMP_InputField>();
        input.textViewport = vpRT;
        input.textComponent = text;
        input.placeholder = ph;
        input.readOnly = false;
        input.interactable = true;

        input.caretBlinkRate = 0.75f;
        input.caretWidth = 2;
        input.customCaretColor = true;
        input.caretColor = Color.black;
        input.selectionColor = new Color(0f, 0f, 0f, 0.25f);

        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.characterLimit = 24;
        input.pointSize = 28;

        return input;
    }
}
