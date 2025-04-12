using UnityEngine;

public class CharacterController : MonoBehaviour
{
    // ============ �̵� ���� ============
    [Header("Movement Settings")]
    [Tooltip("ĳ������ �ִ� �̵� �ӵ�")]
    public float maxSpeed = 5f;
    [Tooltip("���鿡�� ���� �ð�: �ִ�ӵ����� �����ϴ� �ð�")]
    public float accelerationTime = 0.1f;
    [Tooltip("���鿡�� ���� �ð�: �ִ�ӵ����� �������� �ɸ��� �ð�")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("���� ���� ���� (0 = ���� ���� �Ұ�, 1 = ���� ���۰� ����)")]
    public float airControl = 0.5f;

    // ============ ���� ���� ============
    [Header("����")]
    [Tooltip("�ִ� ���� ����")]
    public float maxJumpHeight = 8f;
    [SerializeField, Range(0.2f, 1.25f)][Tooltip("�ִ� ���̱��� �ɸ��� �ð�")] 
    public float timeToJumpApex = 0.2f;
    [Tooltip("���� ���� ��� ����")]
    public bool allowDoubleJump = false;
    [Tooltip("���� ���� ��� Ƚ��")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f)][Tooltip("������ �ö󰥶� �߷�")] 
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)][Tooltip("������ �����ö� �߷�(������ ���� ������)")] 
    public float downwardMovementMultiplier = 1.17f;
    [Tooltip("�ڿ��� Ÿ��: ������ ��� �Ŀ��� ���� �Է��� �޴� �ð�")]
    public float coyoteTime = 0.2f;
    [Tooltip("���� ����: ���� �Է��� �̸� �����ϴ� �ð�")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(1f, 10f)][Tooltip("������ �����̽��ٿ��� ���� ������ �߷�")] 
    public float jumpCutOff;
    // ============ ���� ���� ============
    [Header("IsGrounded")]
    [Tooltip("���� üũ ��ġ (���� ĳ���� �� �Ʒ��� ��ġ�� Transform)")]
    public Transform groundCheck;
    [Tooltip("���� üũ ���� ������")]
    private float groundCheckDistance = 0.5f;
    [Tooltip("� ���̾ �������� ��������")]
    public LayerMask groundLayer;

    // ============ ���� ���� ���� ============
    private Rigidbody rb;
    public bool isGrounded = false;

    [Header("���κ���")]
    public float jumpSpeed;
    public float defaultGravityScale;
    public float gravityMultiplier;
    public bool variablejumpHeight = true;
    [SerializeField][Tooltip("�������� �ӵ�����")] public float speedLimit;

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

        //����� ���߿� ���� ���� �� ���� �ӵ� ����
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
            Debug.Log("����");
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
                //�߶�
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
