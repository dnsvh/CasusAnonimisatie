#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class CasusBootstrapperMenu {
    const string ScenesFolder = "Assets/Scenes";

    [MenuItem("Tools/CasusAnonimisatie/Add Bootstrapper to City Scenes")]
    public static void AddBootstrapper() {
        AddTo("TheCity");
        AddTo("TheCityAnon");
        EditorUtility.DisplayDialog("Bootstrapper", "GameBootstrapper toegevoegd aan TheCity & TheCityAnon.", "OK");
    }

    static void AddTo(string sceneName) {
        var path = $"{ScenesFolder}/{sceneName}.unity";
        if (!File.Exists(path)) return;
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        if (Object.FindFirstObjectByType<GameBootstrapper>() == null) {
            new GameObject("Bootstrap").AddComponent<GameBootstrapper>();
        }
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
