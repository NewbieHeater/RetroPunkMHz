using UnityEngine;

public class JumpController
{
    private CharacterController cc;
    private Animator animator;

    // 설정값
    public float maxJumpHeight = 10f;
    [Range(0.1f, 1f)] public float timeToJumpApex = 0.5f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public bool allowDoubleJump = false;
    public int maxAirJumps = 1;
    [Range(0f, 5f)] public float upwardMovementMultiplier = 1.2f;
    [Range(1f, 10f)] public float downwardMovementMultiplier = 0.8f;
    [Range(0.5f, 3f)] public float jumpCutOffMultiplier = 2f;
    public float speedLimit = 15f;

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
        float multiplier;
        if (verticalVelocity > 0f)
        {
            // 상승 중
            multiplier = jumpHeld ? upwardMovementMultiplier : jumpCutOffMultiplier;
        }
        else
        {
            // 하강(점프 이후 또는 낭떠러지) 중
            multiplier = downwardMovementMultiplier;
        }
        verticalVelocity += gravityValue * multiplier * dt;

        // Clamp fall speed
        verticalVelocity = Mathf.Clamp(verticalVelocity, -speedLimit, float.MaxValue);

        // Animator
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Fall", verticalVelocity < -0.01f && !isGrounded);

        return verticalVelocity;
    }

    public void OnCeilingHit()
    {
        verticalVelocity = 0f;
        // (원하면 cutOffApplied = true; 도 여기서 걸어줄 수 있습니다)
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