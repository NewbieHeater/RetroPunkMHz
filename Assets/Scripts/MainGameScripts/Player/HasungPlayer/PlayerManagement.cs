using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerManagement : MonoBehaviour
{
    // Sub-Components
    private MovementController movementController;
    private JumpController jumpController;
    private AttackController attackController;
    private GroundDetector groundDetector;
    public PlayerHp playerHp;

    public bool IsGrounded;
    public bool IsEnabled = true;

    void Awake()
    {
        // Ensure all sub-components are present on the same GameObject
        groundDetector = GetComponent<GroundDetector>();
        movementController = GetComponent<MovementController>();
        jumpController = GetComponent<JumpController>();
        attackController = GetComponent<AttackController>();
        playerHp = GetComponent<PlayerHp>();

        // Pass references as needed
        movementController.Initialize(groundDetector);
        jumpController.Initialize(groundDetector);
        attackController.Initialize();
    }

    void Update()
    {
        if(!IsEnabled) return;
        // Centralized Input Handling & UI
        jumpController.HandleInput();
        attackController.HandleInput();

        // Update animation states that depend on frame-based logic
        jumpController.UpdateAnimationStates();
        movementController.UpdateAnimationStates();
    }

    void FixedUpdate()
    {
        if (!IsEnabled) return;
        // Ground detection always updates first
        groundDetector.UpdateGroundStatus();
        bool isGrounded = groundDetector.IsGrounded;
        RaycastHit groundHit = groundDetector.LastHit;
        // Movement physics
        movementController.ProcessMovement(isGrounded, groundHit);
        // Jump physics
        jumpController.ProcessJump(isGrounded);

        IsGrounded = isGrounded;

        // Attack logic (overlap checks)
        attackController.ProcessAttack();
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        movementController.ForceStop();
    }
}