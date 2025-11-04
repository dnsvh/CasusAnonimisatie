using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

public class StartController : MonoBehaviour
{
    [SerializeField] public TMP_Text pressAnyKey;

    void Update()
    {
        // Accept keyboard, mouse, or gamepad "any key" style presses
        bool keyboard = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        bool mouse = false;
        if (Mouse.current != null)
        {
            mouse = Mouse.current.leftButton.wasPressedThisFrame
                 || Mouse.current.rightButton.wasPressedThisFrame
                 || Mouse.current.middleButton.wasPressedThisFrame
                 || Mouse.current.forwardButton.wasPressedThisFrame
                 || Mouse.current.backButton.wasPressedThisFrame;
        }

        bool gamepad = false;
        if (Gamepad.current != null)
        {
            var gp = Gamepad.current;
            gamepad = gp.buttonSouth.wasPressedThisFrame   // A / Cross
                   || gp.buttonEast.wasPressedThisFrame    // B / Circle
                   || gp.buttonNorth.wasPressedThisFrame   // Y / Triangle
                   || gp.buttonWest.wasPressedThisFrame    // X / Square
                   || gp.startButton.wasPressedThisFrame
                   || gp.selectButton.wasPressedThisFrame
                   || gp.leftShoulder.wasPressedThisFrame
                   || gp.rightShoulder.wasPressedThisFrame;
        }

        if (keyboard || mouse || gamepad)
        {
            // Go to Briefing scene
            SceneManager.LoadScene("Briefing");
        }
    }
}
