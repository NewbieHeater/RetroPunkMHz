using UnityEngine;

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
    private IPlayerAnimatorView _anim;

    // 상태
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _jumpHeld;
    private bool _requestCutoff;
    private int _airJumpsLeft;

    private float _baseGravity; // 기본중력
    private bool _wasGrounded;

    public void Initialize(GroundDetector gd, IPlayerAnimatorView anim)
    {
        _rb = GetComponent<Rigidbody>();
        _ground = gd;
        _anim = anim;

        _airJumpsLeft = maxAirJumps;
        _baseGravity = (-2f) / (timeToJumpApex * timeToJumpApex);
    }

    /// 지속적인 입력을 확인하는 함수
    public void OnUpdate(float fdt, PlayerInputFrame input)
    {
        if (input.buttons.IsDown(InputAction.Jump))
        {
            _jumpHeld = true;
            _jumpBufferTimer = jumpBufferTime; 
        }
        if (input.buttons.IsUp(InputAction.Jump))
        {
            _jumpHeld = false;
            _requestCutoff = true;
        }
    }
    // 추가 필드
    private float _fallTimer;
    public float minFallTime = 0.1f; // 최소 낙하 시간 (0.1~0.2f 정도 추천)

    /// 물리적용
    public void OnFixedStep(float fdt, PlayerInputFrame input)
    {
        bool grounded = _ground.IsGrounded;

        UpdateCoyote(grounded, fdt);
        TryConsumeBufferedJump(grounded);

        ApplyGravity(grounded, fdt);
        ApplyCutoffIfRequested();

        ClampVertical();

        bool isFalling = _rb.velocity.y < -0.01f && !_ground.IsGrounded;

        // 짧게 뜬 것은 무시하도록 타이머 사용
        if (isFalling)
        {
            _fallTimer += fdt;
        }
        else
        {
            _fallTimer = 0f;
        }

        bool shouldShowFalling = _fallTimer > minFallTime;

        _anim?.SetFalling(shouldShowFalling);
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
            _jumpBufferTimer = 0f;
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

        // 공중에서 더블점프시 카운트 -1
        if (!grounded && _coyoteTimer <= 0f && _airJumpsLeft > 0)
            _airJumpsLeft--;
    }
    public bool GravityEnabled { get; set; } = true;
    private void ApplyGravity(bool grounded, float dt)
    {
        if (!GravityEnabled) return;
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
