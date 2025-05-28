using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ======================================================
// PlayerController.cs (single entry point for Unity callbacks)
// ======================================================
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.2f;
    [Range(0f, 1f)] public float airControl = 0.5f;
    [Tooltip("회전 속도")] public float rotationSpeed = 0.2f;

    [Header("Jump Settings")]
    public float maxJumpHeight = 4f;
    [Range(0.2f, 1.25f)] public float timeToJumpApex = 0.4f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public bool allowDoubleJump = false;
    public int maxAirJumps = 1;

    [Header("Gravity Multipliers")]
    [Range(0f, 5f)] public float upwardMovementMultiplier = 1f;
    [Range(1f, 10f)] public float downwardMovementMultiplier = 6.17f;
    [Range(2f, 8f)] public float jumpCutOff = 2f;
    public float speedLimit = 15f;

    [Header("Attack Settings")]
    public float normalDamage = 10f;
    public float minChargeTime = 0.2f;
    public float maxChargeTime = 1f;
    public float attackRange = 2f;
    public float attackRadius = 0.5f;
    public Image hpBar;
    public GameObject hpBarParent;
    public TextMeshProUGUI chargedValue;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    // Internal references & handlers
    private Rigidbody rb;
    private MovementHandler movement;
    private JumpHandler jump;
    private GravityHandler gravity;
    private AttackHandler attack;
    private GroundCheckHandler groundCheck;
    private float inputX;

    public Transform mesh;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = new GroundCheckHandler(groundCheckPoint, groundCheckDistance, groundLayer);
        movement = new MovementHandler(rb, maxSpeed, accelerationTime, decelerationTime, airControl, rotationSpeed, mesh);
        jump = new JumpHandler(rb, maxJumpHeight, timeToJumpApex, coyoteTime, jumpBufferTime, allowDoubleJump, maxAirJumps);
        gravity = new GravityHandler(rb, timeToJumpApex, upwardMovementMultiplier, downwardMovementMultiplier, jumpCutOff, speedLimit);
        attack = new AttackHandler(transform, Camera.main, hpBarParent, hpBar, chargedValue, normalDamage, minChargeTime, maxChargeTime, attackRange, attackRadius);
    }

    void Update()
    {
        // Ground check once per frame for input decisions
        groundCheck.Tick();
        bool isGrounded = groundCheck.IsGrounded;

        // Movement input
        inputX = Input.GetAxisRaw("Horizontal");

        // Jump input
        bool jumpDown = Input.GetButtonDown("Jump");
        bool jumpUp = Input.GetButtonUp("Jump");
        jump.ProcessInput(jumpDown, jumpUp);

        // Attack input
        attack.ProcessInput();
    }

    void FixedUpdate()
    {
        bool isGrounded = groundCheck.IsGrounded;

        // Movement physics
        movement.HandleMovement(inputX, isGrounded);

        // Jump physics
        jump.FixedTick(isGrounded);

        // Gravity physics
        gravity.FixedTick(jump.IsJumping, jump.IsHoldingJump, isGrounded);
    }
}
