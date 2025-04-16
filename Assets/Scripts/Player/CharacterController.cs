using Unity.VisualScripting;
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
    [Header("Jump Settings")]
    [Tooltip("최대 점프 높이")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f)]
    [Tooltip("최대 높이까지 걸리는 시간")]
    public float timeToJumpApex;
    [Tooltip("이중 점프 허용 여부")]
    public bool allowDoubleJump = false;
    [Tooltip("이중 점프 허용 횟수")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f)]
    [Tooltip("점프시 올라갈때 중력")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)]
    [Tooltip("점프시 내려올때 중력(높으면 빨리 내려옴)")]
    public float downwardMovementMultiplier = 6.17f;
    [Tooltip("코요테 타임: 지면을 벗어난 후에도 점프 입력을 받는 시간")]
    public float coyoteTime = 0.2f;
    [Tooltip("점프 버퍼: 점프 입력을 미리 저장하는 시간")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(1f, 10f)]
    [Tooltip("점프중 스페이스바에서 손을 놓을때 중력")]
    public float jumpCutOff;
    // ============ 지면 판정 ============
    [Header("Ground Detection")]
    [Tooltip("지면 체크 위치 (보통 캐릭터 발 아래에 위치한 Transform)")]
    public Transform groundCheck;
    [Tooltip("지면 체크 길이 반지름")]
    private float groundCheckDistance = 0.5f;
    [Tooltip("어떤 레이어를 지면으로 간주할지")]
    public LayerMask groundLayer;

    // ============ 내부 상태 변수 ============
    private Rigidbody rb;
    public bool isGrounded = false;

    [Header("Calculations")]
    public float jumpSpeed;
    public float defaultGravityScale;
    public float gravityMultiplier;
    public bool variablejumpHeight = true;
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float speedLimit;
    // 입력 상태
    private float inputX = 0f;
    private float jumpBufferCounter = 0;
    private bool jumpHeld = false;
    [SerializeField]
    private bool desiredJump = false;
    private bool mouseQueued = false;
    private bool currentlyJumping = false;
    public Vector3 velocity;
    public Vector3 horVelocity;
    private float coyoteTimer;
    Vector3 customGravity;
    // 초기화
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //defaultGravityScale = 9.81f;
    }

    void Update()
    {
        
        ProcessInput();
        GroundCheck();
        
        if (jumpBufferTime > 0)
        {
            //Instead of immediately turning off "desireJump", start counting up...
            //All the while, the DoAJump function will repeatedly be fired off
            if (desiredJump)
            {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBufferTime)
                {
                    //If time exceeds the jump buffer, turn off "desireJump"
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }
    }

    // 물리 연산 관련 업데이트: 지면 상태, 점프, 이동 처리
    void FixedUpdate()
    {
        velocity = rb.velocity;
        GroundCheck();
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime;

        

        ProcessMovement();
        if (desiredJump)
        {
            ProcessJump();
            //rb.velocity = velocity;
            //return;
        }

        // 적용한 물리적 계산을 반영
        rb.velocity = velocity;
        CalculateGravity();
        ApplyPhysics();
    }


    void ProcessInput()
    {
        // 수평 이동 입력
        inputX = Input.GetAxisRaw("Horizontal");

        // 점프 입력 처리
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

        // 지면이면 가속률 = maxSpeed / accelerationTime, 공중이면 airControl 배율 적용
        float accelRate = isGrounded ? (maxSpeed / accelerationTime) : (maxSpeed / accelerationTime * airControl);
        float decelRate = isGrounded ? (maxSpeed / decelerationTime) : (maxSpeed / decelerationTime * airControl);

        float newSpeed = 0f;
        if (Mathf.Abs(targetVelocity) > 0.01f)
        {
            // Time.fixedDeltaTime을 곱해서 매 프레임 변화량을 제한합니다.
            newSpeed = Mathf.MoveTowards(velocity.x, targetVelocity, accelRate * Time.fixedDeltaTime);
        }
        else
        {
            newSpeed = Mathf.MoveTowards(velocity.x, 0f, decelRate * Time.fixedDeltaTime);
        }

        // 새로운 수평 속도를 Rigidbody에 반영 (수직속도는 그대로 유지)
        velocity.x = newSpeed;
    }


    private void ProcessJump()
    {
        if (isGrounded || (coyoteTimer > 0.03f && coyoteTimer < coyoteTime) || allowDoubleJump)
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimer = 0;

            //If we have double jump on, allow us to jump again (but only once)
            allowDoubleJump = (maxAirJumps == 1 && allowDoubleJump == false);

            //Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * gravityMultiplier * maxJumpHeight / (timeToJumpApex * 5));

            //If Kit is moving up or down when she jumps (such as when doing a double jump), change the jumpSpeed;
            //This will ensure the jump is the exact same strength, no matter your velocity.
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            velocity.y += jumpSpeed;
            Debug.Log(allowDoubleJump);
            currentlyJumping = true;
        }
        if (jumpBufferTime == 0)
        {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            desiredJump = false;
        }
    }
    void ProcessVariableJump()
    {
        if (rb.velocity.y > 0f)
        {
            // 상승 중이라면 즉시 속도를 절반으로 낮춰 가변 점프 효과 구현
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
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(rb.velocity.y, -speedLimit, 100) , 0);
    }

    public void GroundCheck()
    {

        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    //((2 * maxJumpHeight) * gravityMultiplier) / ((timeToJumpApex * timeToJumpApex) * Physics.gravity.y)
    private void ApplyPhysics()
    {
        Vector3 newGravity = new Vector3(0, 2f * maxJumpHeight * gravityMultiplier / (timeToJumpApex * timeToJumpApex * Physics.gravity.y), 0);
        rb.AddForce(newGravity, ForceMode.Acceleration);
        Debug.Log(newGravity);
        //rb.velocity = horVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // 바닥 판정 영역을 선과 구로 표시합니다.
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayEnd = groundCheck.position + Vector3.down * groundCheckDistance;
            Gizmos.DrawLine(groundCheck.position, rayEnd);
        }
    }
}
