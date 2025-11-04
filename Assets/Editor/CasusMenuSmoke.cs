#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CasusMenuSmoke {
    [MenuItem("Tools/CasusAnonimisatie/SMOKE: Menu alive")]
    public static void Smoke() {
        Debug.Log("CasusAnonimisatie menu is actief ✅");
    }
}
#endif
