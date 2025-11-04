using System.IO;
using UnityEngine;

public static class DataService
{
    // Loads JSON from Resources/Data/*.json
    public static T LoadFromResources<T>(string fileNameWithoutExt)
    {
        var txt = Resources.Load<TextAsset>($"Data/{fileNameWithoutExt}");
        if (txt == null)
        {
            Debug.LogError($"[DataService] Missing Resources/Data/{fileNameWithoutExt}.json");
            return default;
        }
        return JsonUtility.FromJson<T>(txt.text);
    }

    // Save to persistentDataPath (for runtime results like Round B or leaderboard)
    public static void SaveToPersistent<T>(string fileName, T data)
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
#if UNITY_EDITOR
        Debug.Log($"[DataService] Saved {fileName} at {path}");
#endif
    }

    public static T LoadFromPersistent<T>(string fileName)
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path)) return default;
        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }
}
