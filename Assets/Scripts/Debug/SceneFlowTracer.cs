using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowTracer : MonoBehaviour
{
    static SceneFlowTracer _inst;
    void Awake() {
        if (_inst != null) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
        Debug.Log("[SceneFlow] Tracer alive in " + SceneManager.GetActiveScene().name);
    }
    void OnSceneChanged(Scene oldS, Scene newS) {
        Debug.Log($"[SceneFlow] {oldS.name} -> {newS.name}\n{System.Environment.StackTrace}");
    }
}
