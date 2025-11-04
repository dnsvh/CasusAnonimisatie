#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
// Requires Input System package:
using UnityEngine.InputSystem.UI;

public static class AutoSetupCasus {
    const string ScenesFolder = "Assets/Scenes";
    const string PrefabsFolder = "Assets/Prefabs/UI";
    const string ListItemPrefabPath = "Assets/Prefabs/UI/ListItem.prefab";

    [MenuItem("Tools/CasusAnonimisatie/Run Step 3 Auto-Wiring")]
    public static void Run() {
        if (!Directory.Exists(ScenesFolder)) Directory.CreateDirectory(ScenesFolder);
        if (!Directory.Exists(PrefabsFolder)) Directory.CreateDirectory(PrefabsFolder);

        EnsureListItemPrefab();

        CreateStartScene();
        CreateBriefingScene();
        CreateDebriefScene();
        CreateLeaderboardScene();

        // Duplicate TheCity -> TheCityAnon if needed
        var srcCity = Path.Combine(ScenesFolder, "TheCity.unity");
        var dstCityAnon = Path.Combine(ScenesFolder, "TheCityAnon.unity");
        if (File.Exists(srcCity) && !File.Exists(dstCityAnon)) {
            File.Copy(srcCity, dstCityAnon, false);
            AssetDatabase.ImportAsset(dstCityAnon);
        }

        // Inject HUD + Dialogue into TheCity & TheCityAnon
        InjectHUDIntoScene("TheCity");
        InjectHUDIntoScene("TheCityAnon");

        EnsureBuildSettingsOrder(new string[] { "Start","Briefing","TheCity","TheCityAnon","Debrief","Leaderboard" });

        EditorUtility.DisplayDialog("CasusAnonimisatie", "Step 3 auto-wiring completed.", "OK");
    }

    [MenuItem("Tools/CasusAnonimisatie/Fix UI Input Modules (convert all scenes)")]
    public static void FixAllUIInputModules() {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes) {
            if (!s.enabled) continue;
            var scene = EditorSceneManager.OpenScene(s.path, OpenSceneMode.Single);
            ReplaceStandaloneWithInputSystem(scene);
            EditorSceneManager.SaveScene(scene);
        }
        EditorUtility.DisplayDialog("CasusAnonimisatie", "Converted StandaloneInputModule -> InputSystemUIInputModule in all enabled scenes.", "OK");
    }

    // ---------- Scene creation helpers ----------
    static Scene CreateOrOpenScene(string path) {
        if (File.Exists(path)) {
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        } else {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            return scene;
        }
    }

    static void SaveOpenScene(string path) {
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();
    }

    static void EnsureBuildSettingsOrder(string[] sceneNames) {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        foreach (var name in sceneNames) {
            var path = $"{ScenesFolder}/{name}.unity";
            if (File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = list.ToArray();
    }

    // ---------- UI helpers ----------
    static void EnsureEventSystem() {
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es == null) {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        } else {
            // Make sure the module is the InputSystem one
            ReplaceStandaloneWithInputSystem(es.gameObject.scene);
        }
    }

    static void ReplaceStandaloneWithInputSystem(Scene scene) {
        foreach (var root in scene.GetRootGameObjects()) {
            var ess = root.GetComponentsInChildren<EventSystem>(true);
            foreach (var es in ess) {
                var old = es.GetComponent<StandaloneInputModule>();
                if (old != null) Object.DestroyImmediate(old, true);
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
    }

    static Canvas CreateCanvas(string name = "Canvas") {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        return c;
    }

    static GameObject CreateTMPText(Transform parent, string name, int fontSize, TextAlignmentOptions align, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax) {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = align;
        tmp.fontSize = fontSize;
        tmp.text = text;
        return go;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax) {
        var go = new GameObject(name, typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        CreateTMPText(go.transform, "Label", 24, TextAlignmentOptions.Center, label, new Vector2(0,0), new Vector2(1,1), Vector2.zero, Vector2.zero);
        return go.GetComponent<Button>();
    }

    // ---------- Prefab ----------
    static void EnsureListItemPrefab() {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ListItemPrefabPath);
        if (existing != null) return;

        var root = new GameObject("ListItem");
        root.AddComponent<Image>();
        root.AddComponent<Button>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform, false);
        var rect = labelGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(8, 4);
        rect.offsetMax = new Vector2(-8, -4);

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Naam (klik om te beschuldigen)";
        tmp.enableWordWrapping = false;
        tmp.fontSize = 18;

        PrefabUtility.SaveAsPrefabAsset(root, ListItemPrefabPath);
        Object.DestroyImmediate(root);
    }

    // ---------- Scene builders ----------
    static void CreateStartScene() {
        var path = $"{ScenesFolder}/Start.unity";
        CreateOrOpenScene(path);
        var canvas = CreateCanvas();
        EnsureEventSystem();
        var press = CreateTMPText(canvas.transform, "PressAnyKey", 48, TextAlignmentOptions.Center, "Druk op een toets...", new Vector2(0.25f,0.45f), new Vector2(0.75f,0.55f), Vector2.zero, Vector2.zero);

        var holder = new GameObject("_Start");
        var sc = holder.AddComponent<StartController>();
        var so = new SerializedObject(sc);
        so.FindProperty("pressAnyKey").objectReferenceValue = press.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        SaveOpenScene(path);
    }

    static void CreateBriefingScene() {
        var path = $"{ScenesFolder}/Briefing.unity";
        CreateOrOpenScene(path);
        var canvas = CreateCanvas();
        EnsureEventSystem();
        var body = CreateTMPText(canvas.transform, "BodyNL", 28, TextAlignmentOptions.TopLeft, "Briefing...", new Vector2(0.1f,0.1f), new Vector2(0.9f,0.9f), Vector2.zero, Vector2.zero);

        var go = new GameObject("_Briefing");
        var ctrl = go.AddComponent<BriefingController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("bodyNL").objectReferenceValue = body.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        SaveOpenScene(path);
    }

    static void CreateDebriefScene() {
        var path = $"{ScenesFolder}/Debrief.unity";
        CreateOrOpenScene(path);
        var canvas = CreateCanvas();
        EnsureEventSystem();

        var title = CreateTMPText(canvas.transform, "Title", 40, TextAlignmentOptions.Center, "Resultaat", new Vector2(0.2f,0.8f), new Vector2(0.8f,0.92f), Vector2.zero, Vector2.zero);
        var body  = CreateTMPText(canvas.transform, "Explanation", 26, TextAlignmentOptions.TopLeft, "Uitleg...", new Vector2(0.1f,0.5f), new Vector2(0.9f,0.9f), Vector2.zero, Vector2.zero);

        // Nickname panel
        var panel = new GameObject("SavePanel", typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.6f, 0.15f); prt.anchorMax = new Vector2(0.9f, 0.38f);
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        var inputGO = new GameObject("NicknameInput", typeof(TMP_InputField), typeof(TextMeshProUGUI));
        inputGO.transform.SetParent(panel.transform, false);
        var inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0.1f,0.45f); inputRT.anchorMax = new Vector2(0.9f,0.7f);
        var inputText = inputGO.GetComponent<TextMeshProUGUI>(); inputText.text = ""; inputText.fontSize = 24;
        var input = inputGO.GetComponent<TMP_InputField>(); input.textComponent = inputText;

        var saveBtn = CreateButton(panel.transform, "SaveBtn", "Opslaan", new Vector2(0.6f,0.1f), new Vector2(0.9f,0.3f), Vector2.zero, Vector2.zero);

        // DialogueController toast (bottom bar)
        var dlg = CreateDialogueBar(canvas.transform);

        // Wire DebriefController
        var debGO = new GameObject("DebriefController");
        var deb = debGO.AddComponent<DebriefController>();
        var so = new SerializedObject(deb);
        so.FindProperty("title").objectReferenceValue           = title.GetComponent<TextMeshProUGUI>();
        so.FindProperty("explanationNL").objectReferenceValue   = body.GetComponent<TextMeshProUGUI>();
        so.FindProperty("nicknameInput").objectReferenceValue   = input;
        so.FindProperty("saveButtonLabel").objectReferenceValue = saveBtn.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        so.FindProperty("savePanel").objectReferenceValue       = panel;
        so.ApplyModifiedPropertiesWithoutUndo();

        saveBtn.onClick.RemoveAllListeners();
        saveBtn.onClick.AddListener(deb.OnSaveNickname);

        SaveOpenScene(path);
    }

    static void CreateLeaderboardScene() {
        var path = $"{ScenesFolder}/Leaderboard.unity";
        CreateOrOpenScene(path);
        var canvas = CreateCanvas();
        EnsureEventSystem();

        var header = CreateTMPText(canvas.transform, "Header", 36, TextAlignmentOptions.Center, "Ranglijst (Top 10)", new Vector2(0.2f,0.8f), new Vector2(0.8f,0.92f), Vector2.zero, Vector2.zero);
        var body   = CreateTMPText(canvas.transform, "Body", 28, TextAlignmentOptions.TopLeft, "Nog geen tijden.", new Vector2(0.25f,0.2f), new Vector2(0.75f,0.75f), Vector2.zero, Vector2.zero);
        var back   = CreateButton(canvas.transform, "Back", "Terug", new Vector2(0.05f,0.05f), new Vector2(0.18f,0.12f), Vector2.zero, Vector2.zero);

        var ctrlGO = new GameObject("LeaderboardController");
        var ctrl = ctrlGO.AddComponent<LeaderboardController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("header").objectReferenceValue = header.GetComponent<TextMeshProUGUI>();
        so.FindProperty("body").objectReferenceValue   = body.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        back.onClick.RemoveAllListeners();
        back.onClick.AddListener(ctrl.OnBack);

        SaveOpenScene(path);
    }

    // ---------- Injection into play scenes ----------
    static void InjectHUDIntoScene(string sceneName) {
        var path = $"{ScenesFolder}/{sceneName}.unity";
        if (!File.Exists(path)) return;
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        EnsureEventSystem();
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) canvas = CreateCanvas();

        // Dialogue bar (bottom)
        var dlg = CreateDialogueBar(canvas.transform);

        // HUD right side panel
        var hud = new GameObject("HUD");
        hud.transform.SetParent(canvas.transform, false);
        var hudRT = hud.AddComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0.7f, 0.1f); hudRT.anchorMax = new Vector2(0.98f, 0.9f);
        hudRT.offsetMin = Vector2.zero; hudRT.offsetMax = Vector2.zero;

        var timer  = CreateTMPText(hud.transform, "TimerText", 24, TextAlignmentOptions.TopLeft, "Tijd: 300s", new Vector2(0,0.9f), new Vector2(1,1), new Vector2(8,-8), new Vector2(-8,-8));
        var header = CreateTMPText(hud.transform, "ListHeader", 24, TextAlignmentOptions.Left, "Verdachten", new Vector2(0,0.82f), new Vector2(1,0.88f), new Vector2(8,0), new Vector2(-8,0));

        // Scroll area for suspects
        var scrollGO = new GameObject("ScrollView", typeof(Image), typeof(ScrollRect), typeof(Mask));
        scrollGO.transform.SetParent(hud.transform, false);
        var srt = scrollGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0,0.35f); srt.anchorMax = new Vector2(1,0.82f);
        srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
        scrollGO.GetComponent<Mask>().showMaskGraphic = false;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGO.transform, false);
        var vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = new Vector2(0,0); vrt.anchorMax = new Vector2(1,1);
        vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1,1,1,0.03f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0,1); crt.anchorMax = new Vector2(1,1);
        crt.pivot = new Vector2(0.5f,1);
        crt.offsetMin = new Vector2(8,8); crt.offsetMax = new Vector2(-8, -8);
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true; vlg.spacing = 6;
        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollGO.GetComponent<ScrollRect>();
        sr.viewport = viewport.GetComponent<RectTransform>();
        sr.content  = content.GetComponent<RectTransform>();

        var metrics = CreateTMPText(hud.transform, "MetricsText", 22, TextAlignmentOptions.TopLeft, "Privacy-metrics\nk: -\nl: -", new Vector2(0,0.12f), new Vector2(1,0.32f), new Vector2(8,0), new Vector2(-8,0));
        var accuse = CreateButton(hud.transform, "AccuseButton", "Beschuldig", new Vector2(0,0.02f), new Vector2(1,0.1f), Vector2.zero, Vector2.zero);

        // Attach GameHUDController and wire fields
        var ghud = hud.AddComponent<GameHUDController>();
        var so = new SerializedObject(ghud);
        so.FindProperty("timerText").objectReferenceValue   = timer.GetComponent<TextMeshProUGUI>();
        so.FindProperty("listHeader").objectReferenceValue  = header.GetComponent<TextMeshProUGUI>();
        so.FindProperty("metricsText").objectReferenceValue = metrics.GetComponent<TextMeshProUGUI>();
        so.FindProperty("listRoot").objectReferenceValue    = content.transform;
        var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ListItemPrefabPath);
        so.FindProperty("listItemPrefab").objectReferenceValue = itemPrefab;
        so.FindProperty("accuseButton").objectReferenceValue   = accuse;
        so.ApplyModifiedPropertiesWithoutUndo();

        accuse.onClick.RemoveAllListeners();
        accuse.onClick.AddListener(ghud.OnAccuseClicked);

        SaveOpenScene(path);
    }

    static GameObject CreateDialogueBar(Transform parent) {
        var bar = new GameObject("DialogueBar", typeof(Image), typeof(CanvasGroup));
        bar.transform.SetParent(parent, false);
        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,0); rt.anchorMax = new Vector2(1,0.18f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var text = CreateTMPText(bar.transform, "Text", 22, TextAlignmentOptions.TopLeft, "", new Vector2(0.02f,0.15f), new Vector2(0.98f,0.85f), Vector2.zero, Vector2.zero);

        var dlg = bar.AddComponent<DialogueController>();
        var so = new SerializedObject(dlg);
        so.FindProperty("group").objectReferenceValue = bar.GetComponent<CanvasGroup>();
        so.FindProperty("textField").objectReferenceValue = text.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        return bar;
    }
}
#endif
