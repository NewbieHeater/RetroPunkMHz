// RigidMovementController.cs
using UnityEngine;

/// Movement controller: owns ONLY horizontal velocity (x-axis) + facing.
/// No animation/teleport physics inside — uses injected view & GlitchPasser gate.
[RequireComponent(typeof(Rigidbody))]
public class RigidMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxWalkSpeed = 5f;
    public float maxRunSpeed = 8f;
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
    private IPlayerInput _input;
    private IPlayerAnimatorView _anim;
    private Glitch _glitch;

    private bool _isRun = false;
    private float _faceDir = 1f; // -1 or +1 for forward probe direction

    public void Initialize(GroundDetector gd, IPlayerInput input, IPlayerAnimatorView anim, Glitch glitch = null)
    {
        _rb = GetComponent<Rigidbody>();
        _ground = gd;
        _input = input;
        _anim = anim;
        _glitch = glitch;
    }

    public void OnUpdate(float dt)
    {
        if (_input.SprintToggleDown)
        {
            _isRun = !_isRun;
            if (_glitch) _glitch.CanPass = _isRun; // grant pass only when sprint toggled on
        }

        if (Mathf.Abs(_input.MoveX) > 0.01f)
            _faceDir = Mathf.Sign(_input.MoveX);

        // Animation (view)
        bool moving = Mathf.Abs(_input.MoveX) > 0.01f;
        _anim?.SetMove(moving, Mathf.Abs(_rb.velocity.x));
        _anim?.Face(_input.MoveX, rotationSpeed, dt);
    }

    public void OnFixedStep(float dt)
    {
        bool grounded = _ground.IsGrounded;

        // Snap-stop x if nearly zero and grounded to avoid drifting
        if (Mathf.Abs(_input.MoveX) < 0.01f && grounded && Mathf.Abs(_rb.velocity.x) < 0.0005f)
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }

        float maxSpeed = _isRun ? maxRunSpeed : maxWalkSpeed;

        float accel = grounded ? (maxSpeed / accelerationTime)
                               : (maxSpeed / accelerationTime) * airControl;
        float decel = grounded ? (maxSpeed / decelerationTime)
                               : (maxSpeed / decelerationTime) * airControl;

        float targetVx = _input.MoveX * maxSpeed;

        float newVx = (Mathf.Abs(_input.MoveX) > 0.01f)
            ? Mathf.MoveTowards(_rb.velocity.x, targetVx, accel * dt)
            : Mathf.MoveTowards(_rb.velocity.x, 0f, decel * dt);

        if (ProbeForwardBlock())
            newVx = 0f;

        // OWN x ONLY. y is owned by Jump controller.
        _rb.velocity = new Vector3(newVx, Mathf.Clamp(_rb.velocity.y, -verticalSpeedLimit, float.MaxValue), 0f);

        // Auto-cancel run & pass when coming to a stop
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
