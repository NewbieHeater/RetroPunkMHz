using UnityEngine;

public struct DamageInfo
{
    public Vector3 SourceDir;
    public float KnockbackForce;
    public bool IsCharge;
    public int Amount;
}

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class RigidPlayerManagement : MonoBehaviour
{
    [Header("Components (assign or auto-resolve)")]
    private GroundDetector groundDetector;
    private RigidMovementController movementController;
    private RigidJumpController jumpController;
    private AttackController attackController;
    private PlayerAnimatorView animatorView; // IPlayerAnimatorView
    [SerializeField] private Glitch glitchPasser;

    private IPlayerStats stats;

    public bool IsGrounded = false;
    public bool IsEnabled = true;
    public float InputX { get; private set; }

    private void Awake()
    {
        // 스탯 획득
        stats = GetComponent<IPlayerStats>();

        // 비어있으면 가져오기(인스펙터에 지정 시 유지)
        if (!groundDetector) groundDetector = GetComponent<GroundDetector>();
        if (!movementController) movementController = GetComponent<RigidMovementController>();
        if (!jumpController) jumpController = GetComponent<RigidJumpController>();
        if (!attackController) attackController = GetComponent<AttackController>();
        if (!animatorView) animatorView = GetComponentInChildren<PlayerAnimatorView>();
        if (!glitchPasser) glitchPasser = GetComponent<Glitch>();

        // 의존성 주입
        movementController?.Initialize(groundDetector, animatorView, glitchPasser, stats);
        jumpController?.Initialize(groundDetector, animatorView);
        attackController?.Initialize(stats);
    }

    private void Update()
    {
        if (!IsEnabled) return;

        var input = GlobalInputRouter.Instance.CurrentFrame;
        InputX = input.moveX;

        movementController?.OnUpdate(Time.deltaTime, input);
        jumpController?.OnUpdate(Time.deltaTime, input);

        // 중요: 입력은 Update에서
        attackController?.HandleInput();
    }

    private void FixedUpdate()
    {
        if (!IsEnabled) return;

        groundDetector?.UpdateGroundStatus();
        IsGrounded = groundDetector && groundDetector.IsGrounded;

        var input = GlobalInputRouter.Instance.CurrentFrame;

        movementController?.OnFixedStep(Time.fixedDeltaTime, input);
        jumpController?.OnFixedStep(Time.fixedDeltaTime, input);

        // 판정/피직스는 Fixed에서
        attackController?.ProcessAttack();
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        if (!set) movementController?.ForceStop();
    }
}
