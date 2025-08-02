using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
/// <surmmery>
/// 플레이어 관련 스크립트를 모두 관리하는 관리 스크립트
/// </surmmery>
public class RigidPlayerManagement : MonoBehaviour
{
    [SerializeField] private RigidMovementController _movementController;
    [SerializeField] private RigidJumpController _jumpController;
    [SerializeField] private AttackController _attackController;

    public RigidMovementController MovementController => _movementController;
    public RigidJumpController JumpController => _jumpController;
    public AttackController AttackController => _attackController;


    private Rigidbody _rigidBody;
    private CapsuleCollider capsuleCollider;
    private GroundDetector _groundDetector;
    public PlayerHp playerHp;

    public bool IsGrounded;
    public bool IsEnabled = true;

    void Awake()
    {
        // Ensure all sub-components are present on the same GameObject
        _groundDetector = GetComponent<GroundDetector>();
        _movementController = GetComponent<RigidMovementController>();
        _jumpController = GetComponent<RigidJumpController>();
        _attackController = GetComponent<AttackController>();
        //playerHp = GetComponent<PlayerHp>();

        // Pass references as needed
        _movementController.Initialize(_groundDetector);
        _jumpController.Initialize(_groundDetector);
        _attackController.Initialize();
    }

    void Update()
    {
        if (!IsEnabled) return;

        _movementController.HandleInput();
        _jumpController.HandleInput();
        _attackController.HandleInput();

        _jumpController.UpdateAnimationStates();
        _movementController.UpdateAnimationStates();
    }

    void FixedUpdate()
    {
        if (!IsEnabled) return;

        _groundDetector.UpdateGroundStatus();
        bool isGrounded = _groundDetector.IsGrounded;
        RaycastHit groundHit = _groundDetector.LastHit;

        _movementController.ProcessMovement(isGrounded, groundHit, Time.fixedDeltaTime);
        _jumpController.ProcessJump(isGrounded, Time.fixedDeltaTime);

        IsGrounded = isGrounded;

        _attackController.ProcessAttack();
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        _movementController.ForceStop();
    }
}
