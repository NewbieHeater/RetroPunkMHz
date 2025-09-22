using UnityEngine;
using UnityEngine.Windows;

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
    [SerializeField] private GroundDetector groundDetector;
    [SerializeField] private RigidMovementController movementController;
    [SerializeField] private RigidJumpController jumpController;
    [SerializeField] private AttackController attackController;
    [SerializeField] private PlayerAnimatorView animatorView; // IPlayerAnimatorView
    [SerializeField] private Glitch glitchPasser;

    public bool IsGrounded = false;
    public bool IsEnabled = true;
    public float InputX {  get; private set; }

    private void Awake()
    {
        // 비어있으면 가져오기(인스펙터창에 드래그시 그걸로 유지)
        if (!groundDetector) groundDetector = GetComponent<GroundDetector>();
        if (!movementController) movementController = GetComponent<RigidMovementController>();
        if (!jumpController) jumpController = GetComponent<RigidJumpController>();
        if (!attackController) attackController = GetComponent<AttackController>();
        if (!animatorView) animatorView = GetComponentInChildren<PlayerAnimatorView>();
        if (!glitchPasser) glitchPasser = GetComponent<Glitch>();

        // 의존성 주입
        movementController.Initialize(groundDetector, animatorView, glitchPasser);
        jumpController.Initialize(groundDetector, animatorView);
        attackController?.Initialize();
    }

    private void Update()
    {
        if (!IsEnabled) return;

        var input = GlobalInputRouter.Instance.GetFrame();
        InputX = input.moveX;

        movementController?.OnUpdate(Time.deltaTime, input);
        jumpController?.OnUpdate(Time.fixedDeltaTime, input);

        // 나중에 바꿔야함
        // 중요
        // 기억할것
        attackController?.HandleInput();
    }

    private void FixedUpdate()
    {
        if (!IsEnabled) return;

        groundDetector?.UpdateGroundStatus();
        IsGrounded = groundDetector.IsGrounded;

        var input = GlobalInputRouter.Instance.GetFrame();
        movementController?.OnFixedStep(Time.fixedDeltaTime, input);
        jumpController?.OnFixedStep(Time.fixedDeltaTime, input);

        attackController?.ProcessAttack();
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        if (!set) movementController?.ForceStop();
    }
}
