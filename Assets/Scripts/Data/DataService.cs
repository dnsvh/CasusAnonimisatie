using System.IO;
using UnityEngine;

public static class DataService
{
    public static T LoadFromResources<T>(string fileNameWithoutExt) where T : class
    {
        // Try exact path first
        var obj = LoadFromResourcesInternal<T>(fileNameWithoutExt);
        if (obj != null) return obj;

        // Try with/without "Data/" fallback
        if (!fileNameWithoutExt.StartsWith("Data/"))
        {
            obj = LoadFromResourcesInternal<T>("Data/" + fileNameWithoutExt);
            if (obj != null) return obj;
        }
        else
        {
            obj = LoadFromResourcesInternal<T>(fileNameWithoutExt.Replace("Data/", ""));
            if (obj != null) return obj;
        }

        Debug.LogWarning($"[DataService] Resources JSON NOT found for '{fileNameWithoutExt}'. " +
                         $"Tried variations with/without 'Data/'. Ensure file is under Assets/Resources/... and named correctly.");
        return null;
    }

    private static T LoadFromResourcesInternal<T>(string pathNoExt) where T : class
    {
        TextAsset ta = Resources.Load<TextAsset>(pathNoExt);
        if (ta == null) return null;

        try
        {
            var result = JsonUtility.FromJson<T>(ta.text);
            if (result == null)
            {
                Debug.LogWarning($"[DataService] JSON parse returned null for '{pathNoExt}'. " +
                                 $"Check that the JSON structure matches type '{typeof(T).Name}'.");
                return null;
            }

            Debug.Log($"[DataService] Loaded JSON from Resources: '{pathNoExt}'. Bytes={ta.bytes?.Length ?? ta.text.Length}");
            return result;
        }
        catch (System.SystemException ex)
        {
            Debug.LogError($"[DataService] Failed to parse JSON for '{pathNoExt}': {ex.Message}\nContent preview:\n{Preview(ta.text)}");
            return null;
        }
    }

    private static string Preview(string s, int max = 300)
    {
        if (string.IsNullOrEmpty(s)) return "<empty>";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }

    public static void SaveToPersistent<T>(string fileName, T data)
    {
        try
        {
            var json = JsonUtility.ToJson(data, true);
            var path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, json);
            Debug.Log($"[DataService] Saved {fileName} at {path}");
        }
        catch (System.SystemException ex)
        {
            Debug.LogError($"[DataService] SaveToPersistent failed for {fileName}: {ex}");
        }
    }

    public static T LoadFromPersistent<T>(string fileName) where T : class
    {
        try
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            var result = JsonUtility.FromJson<T>(json);
            return result;
        }
        catch (System.SystemException ex)
        {
            Debug.LogError($"[DataService] LoadFromPersistent failed for {fileName}: {ex}");
            return null;
        }
    }
}
