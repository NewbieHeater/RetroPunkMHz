using Unity.VisualScripting;
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
    [Header("Jump Settings")]
    [Tooltip("�ִ� ���� ����")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f)]
    [Tooltip("�ִ� ���̱��� �ɸ��� �ð�")]
    public float timeToJumpApex;
    [Tooltip("���� ���� ��� ����")]
    public bool allowDoubleJump = false;
    [Tooltip("���� ���� ��� Ƚ��")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f)]
    [Tooltip("������ �ö󰥶� �߷�")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)]
    [Tooltip("������ �����ö� �߷�(������ ���� ������)")]
    public float downwardMovementMultiplier = 6.17f;
    [Tooltip("�ڿ��� Ÿ��: ������ ��� �Ŀ��� ���� �Է��� �޴� �ð�")]
    public float coyoteTime = 0.2f;
    [Tooltip("���� ����: ���� �Է��� �̸� �����ϴ� �ð�")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(1f, 10f)]
    [Tooltip("������ �����̽��ٿ��� ���� ������ �߷�")]
    public float jumpCutOff;
    // ============ ���� ���� ============
    [Header("Ground Detection")]
    [Tooltip("���� üũ ��ġ (���� ĳ���� �� �Ʒ��� ��ġ�� Transform)")]
    public Transform groundCheck;
    [Tooltip("���� üũ ���� ������")]
    private float groundCheckDistance = 0.5f;
    [Tooltip("� ���̾ �������� ��������")]
    public LayerMask groundLayer;

    // ============ ���� ���� ���� ============
    private Rigidbody rb;
    public bool isGrounded = false;

    [Header("Calculations")]
    public float jumpSpeed;
    public float defaultGravityScale;
    public float gravityMultiplier;
    public bool variablejumpHeight = true;
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float speedLimit;
    // �Է� ����
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
    // �ʱ�ȭ
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

    // ���� ���� ���� ������Ʈ: ���� ����, ����, �̵� ó��
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

        // ������ ������ ����� �ݿ�
        rb.velocity = velocity;
        CalculateGravity();
        ApplyPhysics();
    }


    void ProcessInput()
    {
        // ���� �̵� �Է�
        inputX = Input.GetAxisRaw("Horizontal");

        // ���� �Է� ó��
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

        // �����̸� ���ӷ� = maxSpeed / accelerationTime, �����̸� airControl ���� ����
        float accelRate = isGrounded ? (maxSpeed / accelerationTime) : (maxSpeed / accelerationTime * airControl);
        float decelRate = isGrounded ? (maxSpeed / decelerationTime) : (maxSpeed / decelerationTime * airControl);

        float newSpeed = 0f;
        if (Mathf.Abs(targetVelocity) > 0.01f)
        {
            // Time.fixedDeltaTime�� ���ؼ� �� ������ ��ȭ���� �����մϴ�.
            newSpeed = Mathf.MoveTowards(velocity.x, targetVelocity, accelRate * Time.fixedDeltaTime);
        }
        else
        {
            newSpeed = Mathf.MoveTowards(velocity.x, 0f, decelRate * Time.fixedDeltaTime);
        }

        // ���ο� ���� �ӵ��� Rigidbody�� �ݿ� (�����ӵ��� �״�� ����)
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
            // ��� ���̶�� ��� �ӵ��� �������� ���� ���� ���� ȿ�� ����
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
            // �ٴ� ���� ������ ���� ���� ǥ���մϴ�.
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayEnd = groundCheck.position + Vector3.down * groundCheckDistance;
            Gizmos.DrawLine(groundCheck.position, rayEnd);
        }
    }
}
