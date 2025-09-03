using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class RigidPlayerManagement : MonoBehaviour
{
    [Header("Components (assign or auto-resolve)")]
    [SerializeField] private GroundDetector groundDetector;
    [SerializeField] private RigidMovementController movementController;
    [SerializeField] private RigidJumpController jumpController;
    [SerializeField] private AttackController attackController;
    [SerializeField] private KeyboardInput keyboardInput; // IPlayerInput
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
        if (!keyboardInput) keyboardInput = GetComponent<KeyboardInput>();
        if (!animatorView) animatorView = GetComponentInChildren<PlayerAnimatorView>();
        if (!glitchPasser) glitchPasser = GetComponent<Glitch>();

        // 의존성 주입
        movementController.Initialize(groundDetector, keyboardInput, animatorView, glitchPasser);
        jumpController.Initialize(groundDetector, keyboardInput, animatorView);
        attackController?.Initialize();
    }

    private void Update()
    {
        if (!IsEnabled) return;

        keyboardInput?.Read();
        InputX = keyboardInput.MoveX;

        movementController?.OnUpdate(Time.deltaTime);
        jumpController?.OnUpdate();

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

        movementController?.OnFixedStep(Time.fixedDeltaTime);
        jumpController?.OnFixedStep(Time.fixedDeltaTime);

        attackController?.ProcessAttack();
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        if (!set) movementController?.ForceStop();
    }
}
