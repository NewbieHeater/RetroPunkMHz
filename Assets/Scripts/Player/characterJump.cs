using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("최대 점프 높이")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f), Tooltip("최대 높이까지 걸리는 시간")]
    public float timeToJumpApex;
    [Tooltip("이중 점프 활성화 여부")]
    public bool doubleJumpActive = false;
    [Tooltip("이중 점프 허용 횟수")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f), Tooltip("점프시 올라갈때 중력")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f), Tooltip("점프시 내려올때 중력(높으면 빨리 내려옴)")]
    public float downwardMovementMultiplier = 6.17f;
    [Tooltip("코요테 타임: 지면을 벗어난 후에도 점프 입력을 받는 시간")]
    public float coyoteTime = 0.2f;
    [Tooltip("점프 버퍼: 점프 입력을 미리 저장하는 시간")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(1f, 10f), Tooltip("점프중 스페이스바에서 손을 놓을때 중력")]
    public float jumpCutOff;
    [Tooltip("최대 자유낙하 속도")]
    public float speedLimit = 20f;

    Rigidbody rb;
    bool desiredJump, jumpHeld;
    float coyoteTimer, jumpBufferCounter;
    bool allowDoubleJump, currentlyJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ProcessInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            jumpHeld = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            if (rb.velocity.y > 0f)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0);
        }
    }

    /// <summary>
    /// 이동 처리 이후, 중력/점프를 velocity에 더해주는 메서드
    /// </summary>
    public void HandlePhysics(bool isGrounded)
    {
        // coyote timer
        coyoteTimer = isGrounded ? coyoteTime : (coyoteTimer - Time.fixedDeltaTime);

        // 버퍼 카운터
        if (desiredJump)
        {
            jumpBufferCounter += Time.fixedDeltaTime;
            if (jumpBufferCounter > jumpBufferTime)
                desiredJump = false;
        }

        Vector3 vel = rb.velocity;

        // 실제 점프 실행
        if (desiredJump && (isGrounded || coyoteTimer > 0f || allowDoubleJump))
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimer = 0f;

            if (!isGrounded && doubleJumpActive && !allowDoubleJump)
                allowDoubleJump = true;

            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * maxJumpHeight);
            if (vel.y > 0f) jumpSpeed = Mathf.Max(jumpSpeed - vel.y, 0f);
            else vel.y = 0;

            vel.y += jumpSpeed;
            currentlyJumping = true;
        }

        // 중력 배수
        float mult = vel.y > 0
            ? (jumpHeld && currentlyJumping ? upwardMovementMultiplier : jumpCutOff)
            : (isGrounded ? 1f : downwardMovementMultiplier);

        rb.AddForce(new Vector3(0, Physics.gravity.y * (mult - 1f), 0), ForceMode.Acceleration);

        // 최대 낙하 속도 제한
        vel = new Vector3(vel.x, Mathf.Clamp(rb.velocity.y, -speedLimit, float.MaxValue), 0);
        rb.velocity = vel;

        CurrentVelocity = vel;
    }

    public Vector3 CurrentVelocity { get; private set; }
}
