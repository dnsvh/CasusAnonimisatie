#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public static class CasusNPCUtilities {

    [MenuItem("Tools/CasusAnonimisatie/Create Simple NPC Prefab")]
    public static void CreateSimpleNPCPrefab() {
        var go = new GameObject("NPC");
        var sr = go.AddComponent<SpriteRenderer>();
        var playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/007_1.png");
        if (playerSprite) sr.sprite = playerSprite;

        go.AddComponent<BoxCollider2D>();
        go.AddComponent<NPCDialogue>();
        go.AddComponent<NPCInteractable>();

        string prefabPath = "Assets/Prefabs/NPC/NPC.prefab";
        System.IO.Directory.CreateDirectory("Assets/Prefabs/NPC");
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go);
        Debug.Log("✅ Simple NPC prefab created at " + prefabPath);
    }

    [MenuItem("Tools/CasusAnonimisatie/Populate NPCs in City Scenes")]
    public static void PopulateNPCs() {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NPC/NPC.prefab");
        if (!prefab) { Debug.LogError("❌ Could not find NPC prefab! Run 'Create Simple NPC Prefab' first."); return; }

        var scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded) { Debug.LogError("Open a city scene first."); return; }

        for (int i = 0; i < 6; i++) {
            var npc = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            npc.name = "NPC_" + i;
            npc.transform.position = new Vector3(Random.Range(-5, 5), Random.Range(-3, 3), 0);
        }
        Debug.Log("✅ Populated NPCs in scene: " + scene.name);
    }
}
#endif
