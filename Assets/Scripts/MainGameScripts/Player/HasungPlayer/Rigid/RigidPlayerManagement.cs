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
    [SerializeField] private KeyboardInput keyboardInput; // implements IPlayerInput
    [SerializeField] private PlayerAnimatorView animatorView; // implements IPlayerAnimatorView
    [SerializeField] private Glitch glitchPasser;

    public bool IsGrounded = false;
    public bool IsEnabled = true;
    public float InputX {  get; private set; }

    private void Awake()
    {
        // Auto-resolve if left empty
        if (!groundDetector) groundDetector = GetComponent<GroundDetector>();
        if (!movementController) movementController = GetComponent<RigidMovementController>();
        if (!jumpController) jumpController = GetComponent<RigidJumpController>();
        if (!attackController) attackController = GetComponent<AttackController>();
        if (!keyboardInput) keyboardInput = GetComponent<KeyboardInput>();
        if (!animatorView) animatorView = GetComponentInChildren<PlayerAnimatorView>();
        if (!glitchPasser) glitchPasser = GetComponent<Glitch>();

        // Dependency injection
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

        // If your attack system needs Update-time input edges, call here too:
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
