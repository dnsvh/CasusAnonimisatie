using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class DebriefController : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text explanationNL;

    // For leaderboard nickname entry (optional UI)
    public TMP_InputField nicknameInput;

    void Start()
    {
        if (title) title.text = DebriefState.LastWin ? "Je hebt gewonnen!" : "Je hebt verloren";
        if (explanationNL)
        {
            explanationNL.text = DebriefState.LastWin
                ? "Goed gedaan. In ronde B waren de hints geanonimiseerd (categorieën / postcode-prefix), dus je had minder precisie."
                : "Lastig hè? In ronde B werden hints geanonimiseerd (leeftijdsbinnen, postcode-prefix), waardoor identificatie moeilijker werd.";
        }
    }

    // Editor auto-setup calls this. It should exist even if you don't use the leaderboard yet.
    // Validates AN001: only Latin letters and Arabic numerals.
    public void OnSaveNickname()
    {
        string nick = nicknameInput ? nicknameInput.text : "";
        nick = (nick ?? "").Trim();

        if (string.IsNullOrEmpty(nick) || !Regex.IsMatch(nick, @"^[A-Za-z0-9]+$"))
        {
            DialogueController.I?.Show("Voer een geldige bijnaam in (alleen letters en cijfers).");
            return;
        }

        // Minimal persistence; replace with your leaderboard service later
        PlayerPrefs.SetString("last_nick", nick);
        PlayerPrefs.Save();

        // Go to Leaderboard scene if you have one; otherwise back to Start
        if (Application.CanStreamedLevelBeLoaded("Leaderboard"))
            SceneManager.LoadScene("Leaderboard");
        else
            SceneManager.LoadScene("Start");
    }

    public void OnBackToStart() => SceneManager.LoadScene("Start");
}
