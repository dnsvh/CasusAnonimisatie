#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class CasusBootstrapCleanup {
    [MenuItem("Tools/CasusAnonimisatie/Cleanup: remove Bootstrapper from non-city scenes")]
    public static void Cleanup() {
        string[] sceneNames = new[] { "Start", "Briefing", "Debrief", "Leaderboard" };
        foreach (var name in sceneNames) {
            string path = $"Assets/Scenes/{name}.unity";
            if (!File.Exists(path)) continue;
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var boot = Object.FindFirstObjectByType<GameBootstrapper>(FindObjectsInactive.Include);
            if (boot != null) {
                Object.DestroyImmediate(boot.gameObject);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Removed GameBootstrapper from {name}");
            }
        }
        // Reopen TheCity so you stay where you were
        var city = "Assets/Scenes/TheCity.unity";
        if (File.Exists(city)) EditorSceneManager.OpenScene(city, OpenSceneMode.Single);
    }
}
#endif
