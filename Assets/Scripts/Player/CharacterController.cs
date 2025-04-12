using UnityEngine;

public class CharacterController : MonoBehaviour
{
    // ============ 이동 설정 ============
    [Header("Movement Settings")]
    [Tooltip("캐릭터의 최대 이동 속도")]
    public float maxSpeed = 5f;
    [Tooltip("지면에서 가속 시간: 최대속도까지 도달하는 시간")]
    public float accelerationTime = 0.1f;
    [Tooltip("지면에서 감속 시간: 최대속도에서 정지까지 걸리는 시간")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("공중 조작 정도 (0 = 공중 조작 불가, 1 = 지면 조작과 동일)")]
    public float airControl = 0.5f;

    // ============ 점프 설정 ============
    [Header("점프")]
    [Tooltip("최대 점프 높이")]
    public float maxJumpHeight = 8f;
    [SerializeField, Range(0.2f, 1.25f)][Tooltip("최대 높이까지 걸리는 시간")] 
    public float timeToJumpApex = 0.2f;
    [Tooltip("이중 점프 허용 여부")]
    public bool allowDoubleJump = false;
    [Tooltip("이중 점프 허용 횟수")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f)][Tooltip("점프시 올라갈때 중력")] 
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)][Tooltip("점프시 내려올때 중력(높으면 빨리 내려옴)")] 
    public float downwardMovementMultiplier = 1.17f;
    [Tooltip("코요테 타임: 지면을 벗어난 후에도 점프 입력을 받는 시간")]
    public float coyoteTime = 0.2f;
    [Tooltip("점프 버퍼: 점프 입력을 미리 저장하는 시간")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(1f, 10f)][Tooltip("점프중 스페이스바에서 손을 놓을때 중력")] 
    public float jumpCutOff;
    // ============ 지면 판정 ============
    [Header("IsGrounded")]
    [Tooltip("지면 체크 위치 (보통 캐릭터 발 아래에 위치한 Transform)")]
    public Transform groundCheck;
    [Tooltip("지면 체크 길이 반지름")]
    private float groundCheckDistance = 0.5f;
    [Tooltip("어떤 레이어를 지면으로 간주할지")]
    public LayerMask groundLayer;

    // ============ 내부 상태 변수 ============
    private Rigidbody rb;
    public bool isGrounded = false;

    [Header("내부변수")]
    public float jumpSpeed;
    public float defaultGravityScale;
    public float gravityMultiplier;
    public bool variablejumpHeight = true;
    [SerializeField][Tooltip("떨어지는 속도제한")] public float speedLimit;

    private float inputX = 0f;
    private float jumpBufferCounter = 0;
    private bool jumpHeld = false;
    private bool desiredJump = false;
    private bool mouseQueued = false;
    private bool currentlyJumping = false;
    public Vector3 velocity;
    public Vector3 horVelocity;
    private float coyoteTimer;
    Vector3 customGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultGravityScale = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        ProcessInput();
        GroundCheck();
        
        if (jumpBufferTime > 0)
        {
            if (desiredJump)
            {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBufferTime)
                {
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }
    }

    void FixedUpdate()
    {
        velocity = rb.velocity;
        horVelocity = rb.velocity;
        ProcessMovement();
        CalculateGravity();
        ApplyPhysics();
        
        if (desiredJump)
        {
            ProcessJump();
            rb.velocity = velocity;
            return;
        }
    }

    void ProcessInput()
    {
        inputX = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            jumpHeld = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            ProcessVariableJump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouseQueued = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouseQueued = false;
        }
    }

    void ProcessMovement()
    {

        float targetVelocity = inputX * maxSpeed;
        float currentVelocity = rb.velocity.x;

        //지면과 공중에 따른 가속 및 감속 속도 결정
        float accelRate = isGrounded ? maxSpeed / accelerationTime : (maxSpeed / airControl);
        float decelRate = isGrounded ? maxSpeed / decelerationTime : (maxSpeed / airControl);

        float newSpeed = 0f;
        if (Mathf.Abs(targetVelocity) > 0.01f)
        {
            newSpeed = Mathf.MoveTowards(horVelocity.x, targetVelocity, accelRate);
        }
        else
        {
            newSpeed = Mathf.MoveTowards(horVelocity.x, 0f, decelRate);
        }
        horVelocity.x = newSpeed;
        //rb.velocity = new Vector3(newSpeed, rb.velocity.y, 0);
    }

    private void ProcessJump()
    {
        if (isGrounded || (coyoteTimer > 0.03f && coyoteTimer < coyoteTime) || allowDoubleJump)
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimer = 0;

            allowDoubleJump = (maxAirJumps == 1 && allowDoubleJump == false);

            jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * gravityMultiplier * maxJumpHeight);

            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.velocity.y);
            }

            velocity.y += jumpSpeed;
            Debug.Log("실행");
            currentlyJumping = true;
        }
        if (jumpBufferTime == 0)
        {
            desiredJump = false;
        }
    }
    void ProcessVariableJump()
    {
        if (rb.velocity.y > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0);
        }
    }
    private void CalculateGravity()
    {
        if (rb.velocity.y > 0.01f)
        {
            if (isGrounded)
            {
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                if (jumpHeld && currentlyJumping)
                {
                    gravityMultiplier = upwardMovementMultiplier;
                }
                else
                {
                    gravityMultiplier = jumpCutOff;
                }
            } 
        }
        else if (rb.velocity.y < 0.01f)
        {
            if (isGrounded)
            {
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                //추락
                gravityMultiplier = downwardMovementMultiplier;
            }
        }
        else if (!jumpHeld && isGrounded)
        {
            gravityMultiplier = defaultGravityScale;
        }
        else
        {
            if (isGrounded)
            {
                currentlyJumping = false;
            }

            gravityMultiplier = defaultGravityScale;
        }
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, 100));
    }
    
    public void GroundCheck()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    private void ApplyPhysics()
    {
        Vector3 newGravity = new Vector3(0, (1 * maxJumpHeight) / (timeToJumpApex * timeToJumpApex), 0);
        rb.AddForce(velocity.x, (newGravity.y / Physics.gravity.y) * gravityMultiplier, 0, ForceMode.Acceleration);
        
        rb.velocity = horVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayEnd = groundCheck.position + Vector3.down * groundCheckDistance;
            Gizmos.DrawLine(groundCheck.position, rayEnd);
        }
    }
}
