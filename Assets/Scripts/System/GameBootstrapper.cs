using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrapper : MonoBehaviour
{
    [Tooltip("Optioneel: forceer startfase (laat anders scene-naam bepalen).")]
    public bool forceRoundA = false;

    void Awake()
    {
        if (GameManager.I == null)
        {
            GameObject root = GameObject.Find("_Game");
            if (root == null) root = new GameObject("_Game");
            var gm = root.GetComponent<GameManager>();
            if (gm == null) gm = root.AddComponent<GameManager>();
        }

        // Set phase based on scene
        if (GameManager.I != null)
        {
            var scene = SceneManager.GetActiveScene().name;
            if (forceRoundA) {
                GameManager.I.phase = RoundPhase.RoundA;
            } else {
                if (scene == "TheCity")     GameManager.I.phase = RoundPhase.RoundA;
                if (scene == "TheCityAnon") GameManager.I.phase = RoundPhase.RoundB;
            }
        }
    }
}
