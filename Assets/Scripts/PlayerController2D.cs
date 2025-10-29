using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 input;

    public Vector2 InputVector { get; private set; } // <-- add this

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f; // top-down
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        input = Vector2.zero;
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed)  input.x -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) input.x += 1f;
            if (k.sKey.isPressed || k.downArrowKey.isPressed)  input.y -= 1f;
            if (k.wKey.isPressed || k.upArrowKey.isPressed)    input.y += 1f;
        }
#else
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
#endif
        input = Vector2.ClampMagnitude(input, 1f);
        InputVector = input; // <-- update this
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}
