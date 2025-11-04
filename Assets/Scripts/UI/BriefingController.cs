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
        if (bodyNL != null) {
            bodyNL.text =
@"<b>Wat is anonimisering?</b>
Persoonsgegevens zo bewerken dat individuen niet meer identificeerbaar zijn. In tegenstelling tot pseudonimisering (waarbij je via een sleutel nog kunt terugvinden wie iemand is) is echte anonimisering onomkeerbaar.

<b>Kernbegrippen</b>
• <b>k-anonimiteit</b>: voor elke combinatie van quasi-identifiers (bv. leeftijd, geslacht, postcode) bestaan ≥ k gelijke records.
• <b>l-diversity</b>: binnen elke k-groep zijn ≥ l verschillende waarden voor gevoelige attributen.
• <b>t-closeness</b>: verdeling van gevoelige attributen per groep lijkt op de verdeling in de hele dataset.

<b>Ronde A</b>: geen anonimisering → specifieker hints.  
<b>Ronde B</b>: geanonimiseerd → hints worden gegeneraliseerd (bv. leeftijd 32 → ""tussen 30-39"").

<b>Besturing</b>
• Bewegen: WASD/pijltjes.  
• Praat met NPC: E.  
• Beschuldig via de lijst rechts.

Druk op een toets of knop om te starten…";
        }
    }

    void Update()
    {
        _elapsed += Time.deltaTime;

        bool keyboard = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mouse    = Mouse.current   != null && (Mouse.current.leftButton.wasPressedThisFrame
                        || Mouse.current.rightButton.wasPressedThisFrame
                        || Mouse.current.middleButton.wasPressedThisFrame
                        || Mouse.current.forwardButton.wasPressedThisFrame
                        || Mouse.current.backButton.wasPressedThisFrame);
        bool gamepad  = Gamepad.current  != null && (
                        Gamepad.current.buttonSouth.wasPressedThisFrame ||
                        Gamepad.current.buttonEast.wasPressedThisFrame  ||
                        Gamepad.current.buttonNorth.wasPressedThisFrame ||
                        Gamepad.current.buttonWest.wasPressedThisFrame  ||
                        Gamepad.current.startButton.wasPressedThisFrame ||
                        Gamepad.current.selectButton.wasPressedThisFrame);

        if (_elapsed >= minSecondsBeforeContinue && (keyboard || mouse || gamepad)) {
            if (GameManager.I != null) GameManager.I.ResetToRoundA(); SceneManager.LoadScene("TheCity");
        }
    }
}

