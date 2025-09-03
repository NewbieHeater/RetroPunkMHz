// RigidJumpController.cs
using UnityEngine;

/// Jump+Gravity controller: owns ONLY vertical(y) velocity & jump state.
/// Uses coyote, buffer, jump-cutoff, configurable multipliers.
[RequireComponent(typeof(Rigidbody))]
public class RigidJumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float maxJumpHeight = 4f;
    [Range(0.2f, 1.25f)] public float timeToJumpApex = 0.4f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Air Jumps (set 0 to disable)")]
    [Min(0)] public int maxAirJumps = 1;

    [Header("Gravity Multipliers")]
    [Range(0f, 5f)] public float upwardMovementMultiplier = 1f;
    [Range(1f, 10f)] public float downwardMovementMultiplier = 6.17f;
    [Range(1.1f, 8f)] public float jumpCutOffMultiplier = 2f;

    [Header("Clamp")]
    public float verticalSpeedLimit = 15f;

    private Rigidbody _rb;
    private GroundDetector _ground;
    private IPlayerInput _input;
    private IPlayerAnimatorView _anim;

    // State
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _jumpHeld;
    private bool _requestCutoff;
    private int _airJumpsLeft;

    private float _baseGravity; // negative accel (units: m/s^2)
    private bool _wasGrounded;

    public void Initialize(GroundDetector gd, IPlayerInput input, IPlayerAnimatorView anim)
    {
        _rb = GetComponent<Rigidbody>();
        _ground = gd;
        _input = input;
        _anim = anim;

        _airJumpsLeft = maxAirJumps;
        _baseGravity = (-2f) / (timeToJumpApex * timeToJumpApex);
    }

    /// Handle button edges in Update (after input.Read()).
    public void OnUpdate()
    {
        if (_input.JumpDown)
        {
            _jumpHeld = true;
            _jumpBufferTimer = jumpBufferTime; // (re)arm buffer
        }
        if (_input.JumpUp)
        {
            _jumpHeld = false;
            _requestCutoff = true;
        }
    }

    /// Physics step in FixedUpdate.
    public void OnFixedStep(float dt)
    {
        bool grounded = _ground.IsGrounded;

        UpdateCoyote(grounded, dt);
        TryConsumeBufferedJump(grounded);

        ApplyGravity(grounded, dt);
        ApplyCutoffIfRequested();

        ClampVertical();

        // Animation view
        bool isFalling = _rb.velocity.y < -0.01f && !grounded;
        _anim?.SetFalling(isFalling);
        _anim?.SetGrounded(grounded);

        _wasGrounded = grounded;
    }

    private void UpdateCoyote(bool grounded, float dt)
    {
        if (grounded)
        {
            _coyoteTimer = coyoteTime;
            _airJumpsLeft = maxAirJumps;
        }
        else
        {
            _coyoteTimer -= dt;
        }

        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= dt;
    }

    private void TryConsumeBufferedJump(bool grounded)
    {
        if (_jumpBufferTimer <= 0f) return;

        if (CanJump(grounded))
        {
            DoJump(grounded);
            _jumpBufferTimer = 0f; // consumed
        }
    }

    private bool CanJump(bool grounded)
    {
        return grounded || _coyoteTimer > 0f || _airJumpsLeft > 0;
    }

    private void DoJump(bool grounded)
    {
        _anim?.TriggerJump();

        _coyoteTimer = 0f;

        float jumpVelocity = Mathf.Sqrt(-2f * _baseGravity * maxJumpHeight);
        _rb.velocity = new Vector3(_rb.velocity.x, jumpVelocity, 0f);

        // If this was an air jump, consume a charge
        if (!grounded && _coyoteTimer <= 0f && _airJumpsLeft > 0)
            _airJumpsLeft--;
    }

    private void ApplyGravity(bool grounded, float dt)
    {
        if (grounded) return;

        float multiplier;
        if (_rb.velocity.y > 0f)
            multiplier = _jumpHeld ? upwardMovementMultiplier : jumpCutOffMultiplier;
        else
            multiplier = downwardMovementMultiplier;

        _rb.AddForce(Vector3.up * _baseGravity * multiplier, ForceMode.Acceleration);
    }

    private void ApplyCutoffIfRequested()
    {
        if (!_requestCutoff) return;
        _requestCutoff = false;

        if (_rb.velocity.y > 0f)
        {
            float newVy = _rb.velocity.y / Mathf.Max(1.0001f, jumpCutOffMultiplier);
            _rb.velocity = new Vector3(_rb.velocity.x, newVy, 0f);
        }
    }

    private void ClampVertical()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, Mathf.Clamp(_rb.velocity.y, -verticalSpeedLimit, float.MaxValue), 0f);
    }
}
