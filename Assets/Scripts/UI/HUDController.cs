using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDController : MonoBehaviour
{
    public static GameHUDController I;

    [Header("Texts")]
    public TMP_Text timerText;
    public TMP_Text listHeader;
    public TMP_Text metricsText;

    [Header("List")]
    public Transform listRoot;
    public GameObject listItemPrefab;

    [Header("Buttons")]
    public Button accuseButton;

    void Awake() { I = this; }

    void Start()
    {
        TryAutoWire();
        StartCoroutine(InitWhenReady());
    }

    System.Collections.IEnumerator InitWhenReady()
    {
        while (GameManager.I == null) yield return null;

        var ui = GameManager.I.settings.ui;
        if (listHeader) listHeader.text = ui.suspectList;

        if (accuseButton)
        {
            var t = accuseButton.GetComponentInChildren<TMP_Text>();
            if (t) t.text = ui.accuse;
            accuseButton.onClick.RemoveAllListeners();
            accuseButton.onClick.AddListener(OnAccuseClicked);
        }

        RefreshSuspectList();
        RefreshMetrics();
    }

    void Update()
    {
        if (GameManager.I != null && timerText)
            timerText.text = $"{GameManager.I.settings.ui.timer}: {Mathf.CeilToInt(GameManager.I.roundTimer)}s";
    }

    public void RefreshSuspectList()
    {
        if (!listRoot || !listItemPrefab || GameManager.I == null) return;

        foreach (Transform c in listRoot) Destroy(c.gameObject);

        foreach (var s in GameManager.I.RemainingSuspects().OrderBy(x => x.name))
        {
            var go = Instantiate(listItemPrefab, listRoot);

            var label = go.GetComponentInChildren<TMP_Text>();
            if (!label) label = go.AddComponent<TextMeshProUGUI>();
            label.text = $"{s.name} ({s.age}, {s.gender}, {s.postcode})";

            var img = go.GetComponent<Image>() ?? go.GetComponentInChildren<Image>() ?? go.AddComponent<Image>();
            img.raycastTarget = true;
            if (img.color.a == 0f) img.color = new Color(1,1,1,0.08f);

            var btn = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>() ?? go.AddComponent<Button>();
            if (btn.targetGraphic == null) btn.targetGraphic = img;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                Debug.Log($"[HUD] Accuse click on {s.id} / {s.name}");
                Accuse(s.id);
            });
        }
    }

    public void RefreshMetrics()
    {
        if (!metricsText || GameManager.I == null) return;
        // TODO: wire real metrics when we align model types
        var remaining = GameManager.I.RemainingSuspects().Count();
        metricsText.text = $"{GameManager.I.settings.ui.metrics}\nOver: {remaining}";
    }

    public void OnAccuseClicked()
    {
        DialogueController.I?.Show("Klik op een naam in de lijst om te beschuldigen.");
    }

    void Accuse(string id)
    {
        GameManager.I?.Accuse(id);
    }

    void TryAutoWire()
    {
        if (!timerText)   timerText   = transform.Find("TimerText")?.GetComponent<TMP_Text>();
        if (!listHeader)  listHeader  = transform.Find("ListHeader")?.GetComponent<TMP_Text>();
        if (!metricsText) metricsText = transform.Find("MetricsText")?.GetComponent<TMP_Text>();
        if (!listRoot)    listRoot    = transform.Find("ScrollView/Viewport/Content");
        if (!accuseButton) accuseButton = transform.Find("AccuseButton")?.GetComponent<Button>();
    }
}
