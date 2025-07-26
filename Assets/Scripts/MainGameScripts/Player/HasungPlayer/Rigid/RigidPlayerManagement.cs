using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class RigidPlayerManagement : MonoBehaviour
{
    public RigidMovementController movementController;
    public RigidJumpController jumpController;
    public AttackController attackController;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private GroundDetector groundDetector;
    public PlayerHp playerHp;

    public bool IsGrounded;
    public bool IsEnabled = true;

    void Awake()
    {
        // Ensure all sub-components are present on the same GameObject
        groundDetector = GetComponent<GroundDetector>();
        movementController = GetComponent<RigidMovementController>();
        jumpController = GetComponent<RigidJumpController>();
        attackController = GetComponent<AttackController>();
        //playerHp = GetComponent<PlayerHp>();

        // Pass references as needed
        movementController.Initialize(groundDetector);
        jumpController.Initialize(groundDetector);
        attackController.Initialize();
    }

    void Update()
    {
        if (!IsEnabled) return;

        movementController.HandleInput();
        jumpController.HandleInput();
        attackController.HandleInput();

        jumpController.UpdateAnimationStates();
        movementController.UpdateAnimationStates();
    }

    void FixedUpdate()
    {
        if (!IsEnabled) return;

        groundDetector.UpdateGroundStatus();
        bool isGrounded = groundDetector.IsGrounded;
        RaycastHit groundHit = groundDetector.LastHit;

        movementController.ProcessMovement(isGrounded, groundHit, Time.fixedDeltaTime);
        jumpController.ProcessJump(isGrounded, Time.fixedDeltaTime);

        IsGrounded = isGrounded;

        attackController.ProcessAttack();
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        movementController.ForceStop();
    }
}
