using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BriefingController : MonoBehaviour
{
    [SerializeField] public TMP_Text bodyNL;
    public float minSecondsBeforeContinue = 0.5f;
    private float _elapsed;

    void Start()
    {
        if (bodyNL != null) // Holy yap below like yap yap yap yap
        {
            bodyNL.text =
@"<b>Welkom, Detective. Twee moorden, twee daders, twee rondes. Spreek getuigen, verzamel hints en wijs de juiste verdachte aan.</b>

<b>Anonimisering, in het kort</b>
Anonimiseren = Gegevens zo bewerken dat iemand niet meer te herkennen is, dankzij een combinatie van <i>anonimiseringstechnieken</i>
Pseudonimiseren = Bepaalde gegevens vervangen door codes. Dat lijkt veilig, maar er bestaat een <i>sleutel</i> naar de echte namen (kluis + sleutel -> omkeerbaar). Anonimiseren is bedoeld als vrijwel onomkeerbaar.

<b>Kernbegrippen (1-zinnen)</b>
• <b>k-anonimiteit</b>: jouw record gaat op in een groep van ≥ k gelijken (bv. 32 -> “30–39”, wijk -> regio).  
• <b>l-diversity</b>: in zo’n k-groep bestaan ≥ l verschillende gevoelige waarden (niet iedereen deelt hetzelfde geheime kenmerk).  
• <b>t-closeness</b>: de verdeling van gevoelige waarden in elke groep lijkt op die van de hele dataset (geen ‘uitschieters’ die je alsnog verraden).

<b>Rondes</b>
• <b>Ronde 1</b> : Scherpe hints, geen anonimisatie toegepast. Deze dader was erg slordig...
• <b>Ronde 2</b> : Hints zijn gegeneraliseerd of ‘wazig’, maar zijn ze wel volledig anoniem..?

<b>Jouw doel</b>
Beschuldig de juiste verdachtes in korte tijd! Succes, Detective, we rekenen op jou.

<b>Besturing</b>
W/A/S/D of pijltjes: bewegen • <b>E</b>: praten • Muis: selecteren • Beschuldig via de lijst rechts.

Druk op een toets of knop om te starten…";

        }
    }

    void Update()
    {
        _elapsed += Time.deltaTime;

        bool keyboard = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mouse = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame
                        || Mouse.current.rightButton.wasPressedThisFrame
                        || Mouse.current.middleButton.wasPressedThisFrame
                        || Mouse.current.forwardButton.wasPressedThisFrame
                        || Mouse.current.backButton.wasPressedThisFrame);
        bool gamepad = Gamepad.current != null && (
                        Gamepad.current.buttonSouth.wasPressedThisFrame ||
                        Gamepad.current.buttonEast.wasPressedThisFrame ||
                        Gamepad.current.buttonNorth.wasPressedThisFrame ||
                        Gamepad.current.buttonWest.wasPressedThisFrame ||
                        Gamepad.current.startButton.wasPressedThisFrame ||
                        Gamepad.current.selectButton.wasPressedThisFrame);

        if (_elapsed >= minSecondsBeforeContinue && (keyboard || mouse || gamepad))
        {
            if (GameManager.I != null) GameManager.I.ResetToRoundA();
            SceneManager.LoadScene("TheCity");
        }
    }
}
