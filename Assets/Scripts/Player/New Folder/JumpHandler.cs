using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpHandler
{
    private Rigidbody rb;
    private float maxJumpHeight, timeToJumpApex, coyoteTime, jumpBufferTime;
    private bool allowDoubleJump;
    private int maxAirJumps;
    private float coyoteTimer, bufferTimer;
    private bool desiredJump, jumpHeld;
    private int airJumpsRemaining;

    public bool IsJumping { get; private set; }
    public bool IsHoldingJump => jumpHeld;

    public JumpHandler(Rigidbody rb, float maxJumpHeight, float timeToJumpApex, float coyoteTime, float jumpBufferTime, bool allowDoubleJump, int maxAirJumps)
    {
        this.rb = rb;
        this.maxJumpHeight = maxJumpHeight;
        this.timeToJumpApex = timeToJumpApex;
        this.coyoteTime = coyoteTime;
        this.jumpBufferTime = jumpBufferTime;
        this.allowDoubleJump = allowDoubleJump;
        this.maxAirJumps = maxAirJumps;
        airJumpsRemaining = maxAirJumps;
    }

    public void ProcessInput(bool jumpDown, bool jumpUp)
    {
        if (jumpDown)
        {
            desiredJump = true;
            jumpHeld = true;
        }
        if (jumpUp)
        {
            jumpHeld = false;
            ApplyJumpCutOff();
        }

        // Buffer
        if (jumpBufferTime > 0 && desiredJump)
        {
            bufferTimer += Time.deltaTime;
            if (bufferTimer > jumpBufferTime)
            {
                desiredJump = false;
                bufferTimer = 0f;
            }
        }
    }

    public void FixedTick(bool isGrounded)
    {
        // Coyote
        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.fixedDeltaTime;

        if (isGrounded) airJumpsRemaining = maxAirJumps;

        if (desiredJump && CanJump())
            ExecuteJump();
    }

    private bool CanJump()
        => coyoteTimer > 0f || (allowDoubleJump && airJumpsRemaining > 0);

    private void ExecuteJump()
    {
        desiredJump = false;
        bufferTimer = 0f;
        coyoteTimer = 0f;
        IsJumping = true;

        // Original jump speed formula
        float newGravity = -2f / (timeToJumpApex * timeToJumpApex);
        float jumpSpeed = Mathf.Sqrt(-9.81f * newGravity * maxJumpHeight / 5f);

        float currentVert = Vector3.Dot(rb.velocity, Vector3.down);
        float targetVert = Mathf.Max(currentVert, jumpSpeed);

        Vector3 v = rb.velocity;
        v.y = targetVert + currentVert;
        rb.velocity = v;

        if (allowDoubleJump)
            airJumpsRemaining--;
    }

    private void ApplyJumpCutOff()
    {
        if (rb.velocity.y > 0f)
        {
            Vector3 v = rb.velocity;
            v.y *= 0.5f;
            rb.velocity = v;
        }
    }

}