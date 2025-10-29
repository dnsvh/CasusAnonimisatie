using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public TextMeshProUGUI remainingText, kText, lText, tText, timerText;
    public Transform clueChipParent;
    public GameObject clueChipPrefab; // simple UI chip with a TMP text
    private float elapsed;

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (timerText) timerText.text = FormatTime(elapsed);
    }

    public void UpdateHud(int remaining, int k, int l, float t, AnonymizationSettings s)
    {
        remainingText.text = $"Remaining: {remaining}";
        kText.text = $"k = {k}";
        lText.text = $"l = {l}";
        tText.text = $"t = {t:0.00}";
        // Optional: color coding
        kText.color = k < s.kTarget ? Color.red : new Color(0.2f,0.8f,0.2f);
        lText.color = l < s.lTarget ? Color.yellow : new Color(0.2f,0.8f,0.2f);
        tText.color = t > s.tThreshold ? Color.yellow : new Color(0.2f,0.8f,0.2f);
    }

    public void ShowClueChip(string key, string value)
    {
        if (!clueChipPrefab || !clueChipParent) return;
        var go = Instantiate(clueChipPrefab, clueChipParent);
        var txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt) txt.text = $"{key}: {value}";
    }

    public void ShowResult(bool win)
    {
        // quick placeholder—replace with a proper panel later
        Debug.Log(win ? "You got the murderer!" : "Wrong suspect.");
    }

    string FormatTime(float s)
    {
        int m = (int)(s/60f);
        int sec = (int)(s % 60f);
        int ms = (int)((s - Mathf.Floor(s)) * 1000);
        return $"{m:00}:{sec:00}.{ms:000}";
    }

    public float GetElapsed() => elapsed;
}
