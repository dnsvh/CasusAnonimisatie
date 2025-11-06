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

    // cache the list font so we don't keep loading it
    static TMP_FontAsset sListFont;

    void Awake() => I = this;

    void Start()
    {
        if (sListFont == null)
        {

            sListFont = Resources.Load<TMP_FontAsset>("Fonts/VT323-Regular SDF");

            if (sListFont == null)
                sListFont = Resources.Load<TMP_FontAsset>("VT323-Regular SDF");
            if (sListFont == null)
                Debug.LogWarning("[HUD] Could not find VT323-Regular SDF in Resources. List will use prefab's font.");
        }

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

            var btn = go.GetComponent<Button>();
            var img = go.GetComponent<Image>() ?? (btn != null ? btn.GetComponent<Image>() : null);
            if (img != null)
            {
                img.color = new Color(1f, 1f, 1f, 0.5f);
            }

            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = $"{s.name} ({s.age}, {s.gender}, {s.postcode})";
                if (sListFont != null) text.font = sListFont;
            }

            string capturedId = s.id;

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
        bool roundB = (GameManager.I.phase == RoundPhase.RoundB);

        var m = MetricsCalculator.ComputeAll(
            remaining,
            killerId: GameManager.I.GetKiller()?.id,
            roundB: roundB
        );

        var ui = GameManager.I.settings?.ui;
        string header = ui?.metrics ?? "Privacy-metrics";
        string kLabel = ui?.kAnon ?? "k";
        string lLabel = ui?.lDiv ?? "l";

        string summary = BuildFriendlySummary(m);

        metricsText.text =
            $"{header}\n" +
            $"{kLabel} (gem/max/dader): {m.kAvg:F1} / {m.kMax} / {Mathf.Max(1, m.killerK)}\n" +
            $"{lLabel} (max/dader): {m.lMax} / {Mathf.Max(1, m.killerL)}\n" +
            $"groups: {m.groupCount}, remaining: {m.remainingCount}\n" +
            summary;
    }

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


    static string BuildFriendlySummary(MetricsCalculator.MetricsResult m)
    {
        string kPhrase =
            (m.killerK <= 1) ? "dader valt op" :
            (m.killerK <= 2) ? "dader valt nog op" :
            "dader gaat op in de massa";

        string lPhrase =
            (m.killerL <= 1) ? "weinig variatie" :
            (m.killerL == 2) ? "enige variatie" :
            "veel variatie";

        return $"(k↑ = meer look-alikes, l↑ = meer variatie) • {kPhrase}; banen: {lPhrase}";
    }
}
