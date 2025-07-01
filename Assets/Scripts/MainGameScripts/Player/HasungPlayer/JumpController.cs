using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float maxJumpHeight = 4f;
    [Range(0.2f, 1.25f)] public float timeToJumpApex = 0.4f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public bool allowDoubleJump = false;
    public int maxAirJumps = 1;
    [Range(0f, 5f)] public float upwardMovementMultiplier = 1f;
    [Range(1f, 10f)] public float downwardMovementMultiplier = 6.17f;
    [Range(2f, 8f)] public float jumpCutOffMultiplier = 2f;
    public float speedLimit = 15f;

    private Rigidbody rb;
    private Animator animator;
    private GroundDetector groundDetector;

    private float coyoteTimer;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool desiredJump;
    private bool cutOffApplied;
    private int airJumpsLeft;
    private float inputJump;
    public float gravityValue;
    private float jumpVelocity;
    public void Initialize(GroundDetector gd)
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        groundDetector = gd;
        airJumpsLeft = maxAirJumps;
        gravityValue = (-2f) / (timeToJumpApex * timeToJumpApex);
    }

    public void HandleInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            cutOffApplied = false;
            jumpHeld = true;
            jumpBufferCounter = jumpBufferTime;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            if (!cutOffApplied)
            {
                ApplyJumpCutOff();
                cutOffApplied = true;
            }
        }
    }

    public void ProcessJump(bool isGrounded)
    {
        UpdateCoyoteTimer(isGrounded);
        HandleJumpBuffer(isGrounded);
        if (desiredJump && CanJump(isGrounded))
            ExecuteJump();

        ApplyGravity(isGrounded);
        ApplyPhysicsClamp();

    }

    private void UpdateCoyoteTimer(bool isGrounded)
    {
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            airJumpsLeft = maxAirJumps;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    private void HandleJumpBuffer(bool isGrounded)
    {
        if (!isGrounded && desiredJump && jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
            if (jumpBufferCounter <= 0f)
            {
                jumpBufferCounter = 0f;
                desiredJump = false;
            }
        }
        if (isGrounded && desiredJump && jumpBufferCounter > 0f)
        {
            desiredJump = false;
            jumpBufferCounter = 0f;
            ExecuteJump();
        }
    }

    private bool CanJump(bool isGrounded)
    {
        return isGrounded || coyoteTimer > 0f || (allowDoubleJump && airJumpsLeft > 0);
    }

    private void ExecuteJump()
    {
        animator.SetTrigger("JUMP");
        desiredJump = false;
        coyoteTimer = 0f;

        jumpVelocity = Mathf.Sqrt(-2f * gravityValue * maxJumpHeight);
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, 0f);
        gravity = jumpVelocity;
        if (!groundDetector.IsGrounded)
            airJumpsLeft--;
    }

    private void ApplyJumpCutOff()
    {
        if (rb.velocity.y > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0f);
        }
    }
    public float gravity;
    private void ApplyGravity(bool isGrounded)
    {
        //if (isGrounded) return;
        float multiplier = rb.velocity.y > 0f ? (jumpHeld ? upwardMovementMultiplier : jumpCutOffMultiplier) : downwardMovementMultiplier;
        if(isGrounded)
        {
            //rb.AddForce(Vector3.up * gravityValue * multiplier / 3f, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(Vector3.up * gravityValue * multiplier, ForceMode.Acceleration);
        }
            
    }

    private void ApplyPhysicsClamp()
    {
        rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -speedLimit, float.MaxValue), 0f);
    }

    public void UpdateAnimationStates()
    {
        bool isFalling = rb.velocity.y < -0.01f;
        bool isGrounded = groundDetector.IsGrounded;
        animator.SetBool("Fall", isFalling && !isGrounded);
        animator.SetBool("Grounded", isGrounded);
    }
}