using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator2D : MonoBehaviour
{
    [Header("Assign 3 sprites per direction: [Walk1, Idle, Walk2]")]
    public Sprite[] forward = new Sprite[3];
    public Sprite[] left    = new Sprite[3];
    public Sprite[] right   = new Sprite[3];
    public Sprite[] back    = new Sprite[3];

    [Header("Settings")]
    public float walkFps = 8f; // how fast the walk cycles

    private SpriteRenderer sr;
    private PlayerController2D controller;

    private Vector2 lastFacing = Vector2.down; // default face down
    private float animTimer;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        controller = GetComponent<PlayerController2D>(); // same GameObject
        if (controller == null)
            controller = GetComponentInParent<PlayerController2D>(); // if you put this on a child
    }

    private void Update()
    {
        Vector2 input = controller != null ? controller.InputVector : Vector2.zero;

        // Pick facing from input; if idle, keep lastFacing
        if (input.sqrMagnitude > 0.0001f)
            lastFacing = input;

        // Choose row by facing (4-way)
        Sprite[] row = forward; // default
        if (Mathf.Abs(lastFacing.x) > Mathf.Abs(lastFacing.y))
            row = lastFacing.x > 0 ? right : left;
        else
            row = lastFacing.y > 0 ? back : forward;

        if (input.sqrMagnitude > 0.0001f)
        {
            // Walking: alternate between frame 0 & 2
            animTimer += Time.deltaTime * walkFps;
            int idx = ((int)animTimer % 2 == 0) ? 0 : 2; // 0,2,0,2...
            sr.sprite = row[idx];
        }
        else
        {
            // Idle: middle frame
            sr.sprite = row[1];
            animTimer = 0f;
        }
    }
}
