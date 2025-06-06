using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // Sub-Components
    private MovementController movementController;
    private JumpController jumpController;
    private AttackController attackController;
    private GroundDetector groundDetector;

    public Vector3 velocity;

    void Awake()
    {
        // Ensure all sub-components are present on the same GameObject
        groundDetector = GetComponent<GroundDetector>();
        movementController = GetComponent<MovementController>();
        jumpController = GetComponent<JumpController>();
        attackController = GetComponent<AttackController>();

        // Pass references as needed
        movementController.Initialize(groundDetector);
        jumpController.Initialize(groundDetector);
        attackController.Initialize();
    }

    void Update()
    {
        // Centralized Input Handling & UI
        jumpController.HandleInput();
        attackController.HandleInput();

        // Update animation states that depend on frame-based logic
        jumpController.UpdateAnimationStates();
        movementController.UpdateAnimationStates();
    }

    void FixedUpdate()
    {
        // Ground detection always updates first
        groundDetector.UpdateGroundStatus();
        bool isGrounded = groundDetector.IsGrounded;
        RaycastHit groundHit = groundDetector.LastHit;

        // Jump physics
        jumpController.ProcessJump(isGrounded);

        // Movement physics
        movementController.ProcessMovement(isGrounded, groundHit);

        // Attack logic (overlap checks)
        attackController.ProcessAttack();
    }
}