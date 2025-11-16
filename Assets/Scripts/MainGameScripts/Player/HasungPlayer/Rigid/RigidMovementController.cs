// RigidMovementController.cs
using Unity.VisualScripting;
using UnityEngine;

/// Movement controller: owns ONLY horizontal velocity (x-axis) + facing.
/// No animation/teleport physics inside — uses injected view & GlitchPasser gate.
[RequireComponent(typeof(Rigidbody))]
public class RigidMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.2f;
    [Range(0f, 1f)] public float airControl = 0.5f;
    public float rotationSpeed = 0.2f;

    [Header("Forward Probe (block check)")]
    [SerializeField] private LayerMask forwardBlockMask;
    [SerializeField] private float probeHeight = 0.5f;
    [SerializeField] private float probeDistance = 0.5f;

    [Header("Clamp")]
    [SerializeField] private float verticalSpeedLimit = 15f;

    private Rigidbody _rb;
    private GroundDetector _ground;
    private IPlayerAnimatorView _anim;
    private Glitch _glitch;
    private PlayerStats _stats;

    private bool _isRun = false;
    private float _faceDir = 1f; // -1 or +1 좌우이동용

    public void Initialize(GroundDetector gd, IPlayerAnimatorView anim, Glitch glitch = null, PlayerStats stats = null)
    {
        _rb = GetComponent<Rigidbody>();
        _ground = gd;
        _anim = anim;
        _glitch = glitch;
        _stats = stats;
    }

    public void OnUpdate(float dt, PlayerInputFrame input)
    {
        if (input.buttons.IsDown(InputAction.SprintToggle))
        {
            _isRun = !_isRun;
            if (_glitch) _glitch.CanPass = _isRun; // 달리기 키가 토글되어있을때만 허용
        }

        if (Mathf.Abs(input.moveX) > 0.01f)
            _faceDir = Mathf.Sign(input.moveX);

        // Animation (view)
        bool moving = Mathf.Abs(input.moveX) > 0.01f;
        _anim?.SetMove(moving, Mathf.Abs(_rb.velocity.x));
        _anim?.Face(input.moveX, rotationSpeed, dt);
    }

    public void OnFixedStep(float fdt, PlayerInputFrame input)
    {
        bool grounded = _ground.IsGrounded;

        // 점프, 정지, 이동방향 변경시 미끄러짐 방지
        if (Mathf.Abs(input.moveX) < 0.01f && grounded && Mathf.Abs(_rb.velocity.x) < 0.0005f)
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }

        float maxSpeed = _isRun ? _stats.RunSpeed : _stats.WalkSpeed;

        float accel = grounded ? (maxSpeed / accelerationTime)
                               : (maxSpeed / accelerationTime) * airControl;
        float decel = grounded ? (maxSpeed / decelerationTime)
                               : (maxSpeed / decelerationTime) * airControl;

        float targetVx = input.moveX * maxSpeed;

        float newVx = (Mathf.Abs(input.moveX) > 0.01f)
            ? Mathf.MoveTowards(_rb.velocity.x, targetVx, accel * fdt)
            : Mathf.MoveTowards(_rb.velocity.x, 0f, decel * fdt);

        if (ProbeForwardBlock())
            newVx = 0f;

        // x만 바꿔줌 y는 점프에서만
        _rb.velocity = new Vector3(newVx, Mathf.Clamp(_rb.velocity.y, -verticalSpeedLimit, float.MaxValue), 0f);

        // 점추면 글리치 종료
        if (Mathf.Abs(_rb.velocity.x) <= 0.1f)
        {
            _isRun = false;
            if (_glitch) _glitch.CanPass = false;
        }
    }

    private bool ProbeForwardBlock()
    {
        Vector3 origin = transform.position + Vector3.up * probeHeight;
        Vector3 dir = Vector3.right * _faceDir; // local +x/-x
        return Physics.Raycast(origin, dir, probeDistance, forwardBlockMask, QueryTriggerInteraction.Ignore);
    }

    public void ForceStop()
    {
        if (!_rb) _rb = GetComponent<Rigidbody>();
        _rb.velocity = Vector3.zero;
        _anim?.SetMove(false, 0f);
    }
}
