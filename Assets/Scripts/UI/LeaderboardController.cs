using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] TMP_Text header;
    [SerializeField] TMP_Text body;

    void Start()
    {
        var ui = DataService.LoadFromResources<GameSettings>("settings").ui;
        header.text = ui.leaderboard;

        var t = LeaderboardManager.Load();
        var sb = new StringBuilder();
        int i = 1;
        foreach (var s in t.scores)
        {
            sb.AppendLine($"{i,2}. {s.name} — {s.time:F1}s");
            i++;
        }
        body.text = sb.Length > 0 ? sb.ToString() : "Nog geen tijden.";
    }

    public void OnBack()
    {
        SceneManager.LoadScene(SceneNames.Start);
    }
}
