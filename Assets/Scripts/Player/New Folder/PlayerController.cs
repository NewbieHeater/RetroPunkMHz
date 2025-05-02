using UnityEngine;

[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(GroundChecker))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    [SerializeField]
    private MovementController movement;
    private JumpController jump;
    //private AttackController attack;
    private GroundChecker ground;
    private GravityController gravity;
    private Rigidbody rb;
    private Animator animator;
    [SerializeField]
    private Vector3 velocity;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        if (input == null)
            Debug.LogError("PlayerInput ������Ʈ�� ã�� �� �����ϴ�!");
        movement = GetComponent<MovementController>();
        if (movement == null)
            Debug.LogError("MovementController ������Ʈ�� ã�� �� �����ϴ�!");
        jump = GetComponent<JumpController>();
        //attack = GetComponent<AttackController>();
        ground = GetComponent<GroundChecker>();
        if (ground == null)
            Debug.LogError("GroundChecker ������Ʈ�� ã�� �� �����ϴ�!");
        gravity = GetComponent<GravityController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        gravity.Init(jump.maxJumpHeight, jump.timeToJumpApex);
        //velocity = new Vector3(0,0,0);
    }

    void Update()
    {
        input.HandleInput();
        ground.CheckGround();
        movement.HandleRotation(input.Horizontal);
        //attack.HandleAttack(input.PrimaryAttack, input.SecondaryAttackHeld, input.ChargeTimer);
    }

    void FixedUpdate()
    {
        gravity.ApplyGravity(rb, ground.IsGrounded);
        velocity = rb.velocity;
        movement.ProcessMovement(input.Horizontal, ground.IsGrounded, ref velocity);

        if (jump.TryJump(input.JumpPressed, ground.IsGrounded, ref velocity))
            input.ConsumeJump();
        rb.velocity = velocity;
        gravity.ClampFallVelocity(rb);
        
    }
}