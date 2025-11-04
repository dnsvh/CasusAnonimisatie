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
    public GameObject listItemPrefab; // Button + TMP_Text

    [Header("Buttons")]
    public Button accuseButton;

    void Awake() => I = this;

    void Start()
    {
        var ui = GameManager.I?.settings?.ui;
        if (ui != null)
        {
            listHeader.text = ui.suspectList;
            var btnLabel = accuseButton != null ? accuseButton.GetComponentInChildren<TMP_Text>() : null;
            if (btnLabel) btnLabel.text = ui.accuse;
        }

        if (accuseButton != null)
        {
            accuseButton.onClick.RemoveAllListeners();
            accuseButton.onClick.AddListener(OnAccuseClicked);
        }

        RefreshSuspectList();
        RefreshMetrics();
    }

    void Update()
    {
        if (GameManager.I == null) return;

        GameManager.I.TickTimer(Time.deltaTime);
        var ui = GameManager.I.settings?.ui;
        if (timerText != null)
        {
            string label = ui != null ? ui.timer : "Tijd";
            timerText.text = $"{label}: {Mathf.CeilToInt(GameManager.I.roundTimer)}s";
        }
    }

    public void RefreshSuspectList()
    {
        if (GameManager.I == null || listRoot == null || listItemPrefab == null) return;

        foreach (Transform c in listRoot) Destroy(c.gameObject);

        foreach (var s in GameManager.I.RemainingSuspects().OrderBy(x => x.name))
        {
            var go = Instantiate(listItemPrefab, listRoot);
            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = $"{s.name} ({s.age}, {s.gender}, {s.postcode})";

            // Capture the id in a local to avoid closure issues
            string capturedId = s.id;

            // If the prefab has a Button, clicking the row accuses that suspect
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[HUD] Row clicked -> accuse {capturedId}");
                    Accuse(capturedId);
                });
            }
        }
    }

    public void RefreshMetrics()
    {
        if (GameManager.I == null || metricsText == null) return;

        var remaining = GameManager.I.RemainingSuspects().ToList();
        var (k, l) = MetricsCalculator.KLForDataset(remaining);
        var ui = GameManager.I.settings?.ui;

        string header = ui != null ? ui.metrics : "Metrics";
        string kAnon = ui != null ? ui.kAnon : "k-anon";
        string lDiv = ui != null ? ui.lDiv : "l-div";

        metricsText.text = $"{header}\n{kAnon}: {k}\n{lDiv}: {l}";
    }

    // Beschuldig-knop: als er nog precies 1 over is, beschuldig die automatisch.
    // Anders: toon duidelijke instructie.
    public void OnAccuseClicked()
    {
        if (GameManager.I == null) return;

        var remaining = GameManager.I.RemainingSuspects().ToList();
        if (remaining.Count == 1)
        {
            string id = remaining[0].id;
            Debug.Log($"[HUD] Accuse button pressed with single remaining -> accuse {id}");
            Accuse(id);
        }
        else
        {
            DialogueController.I?.Show(
                remaining.Count == 0
                    ? "Er is niemand meer over om te beschuldigen."
                    : "Klik op een naam in de lijst om die persoon te beschuldigen."
            );
        }
    }

    void Accuse(string id)
    {
        if (string.IsNullOrEmpty(id) || GameManager.I == null) return;
        GameManager.I.Accuse(id);
    }
}
