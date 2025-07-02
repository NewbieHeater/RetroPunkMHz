using UnityEngine;

public class JumpController
{
    private CharacterController cc;
    private Animator animator;

    // 설정값
    public float maxJumpHeight = 10f;
    [Range(0.1f, 1f)] public float timeToJumpApex = 0.5f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public bool allowDoubleJump = false;
    public int maxAirJumps = 1;
    [Range(0f, 5f)] public float upwardMovementMultiplier = 1f;
    [Range(1f, 10f)] public float downwardMovementMultiplier = 6.17f;
    [Range(0.5f, 3f)] public float jumpCutOffMultiplier = 2f;
    public float speedLimit = 20f;

    // 내부 상태
    private float gravityValue;
    private float verticalVelocity;
    private float coyoteTimer;
    private float jumpBufferCounter;
    private bool desiredJump;
    private bool jumpHeld;
    private bool cutOffApplied;
    private int airJumpsLeft;

    public JumpController(CharacterController controller)
    {
        cc = controller;
        animator = controller.GetComponentInChildren<Animator>();
    }

    public void Initialize()
    {
        gravityValue = -2f * maxJumpHeight / (timeToJumpApex * timeToJumpApex);
        airJumpsLeft = maxAirJumps;
    }

    public void HandleInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            jumpHeld = true;
            cutOffApplied = false;
            jumpBufferCounter = jumpBufferTime;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            if (!cutOffApplied && verticalVelocity > 0)
            {
                verticalVelocity *= 0.5f;
                cutOffApplied = true;
            }
        }
    }

    public float ProcessJump(bool isGrounded, float dt)
    {
        // Coyote timer
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            airJumpsLeft = maxAirJumps;
        }
        else
        {
            coyoteTimer -= dt;
        }

        // Jump buffer
        if (desiredJump)
        {
            jumpBufferCounter -= dt;
            if (jumpBufferCounter <= 0f)
                desiredJump = false;
        }

        // Execute jump
        if (desiredJump && (isGrounded || coyoteTimer > 0f || (allowDoubleJump && airJumpsLeft > 0)))
        {
            verticalVelocity = Mathf.Sqrt(-2f * gravityValue * maxJumpHeight);
            animator.SetTrigger("JUMP");
            desiredJump = false;
            coyoteTimer = 0f;
            if (!isGrounded) airJumpsLeft--;
        }

        // Gravity
        float multiplier = verticalVelocity > 0
            ? (jumpHeld ? upwardMovementMultiplier : jumpCutOffMultiplier)
            : downwardMovementMultiplier;
        verticalVelocity += gravityValue * multiplier * dt;

        // Clamp fall speed
        verticalVelocity = Mathf.Clamp(verticalVelocity, -speedLimit, float.MaxValue);

        // Animator
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Fall", verticalVelocity < -0.01f && !isGrounded);

        return verticalVelocity;
    }

    public void UpdateAnimator()
    {
        // (좌우 이동 애니메이션과 혼합 시 필요하다면 여기에)
    }

    public void Reset()
    {
        verticalVelocity = 0f;
        airJumpsLeft = maxAirJumps;
    }
}