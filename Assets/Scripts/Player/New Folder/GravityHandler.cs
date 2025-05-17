using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityHandler
{
    private Rigidbody rb;
    private float timeToJumpApex, upMul, downMul, cutOffMul, speedLimit;

    public GravityHandler(Rigidbody rb, float timeToJumpApex, float upMul, float downMul, float cutOffMul, float speedLimit)
    {
        this.rb = rb;
        this.timeToJumpApex = timeToJumpApex;
        this.upMul = upMul;
        this.downMul = downMul;
        this.cutOffMul = cutOffMul;
        this.speedLimit = speedLimit;
    }

    public void FixedTick(bool isJumping, bool jumpHeld, bool isGrounded)
    {
        float newGravity = -2f / (timeToJumpApex * timeToJumpApex);
        float mult = 1f;

        if (rb.velocity.y > 0.01f)
            mult = (jumpHeld && isJumping) ? upMul : cutOffMul;
        else if (rb.velocity.y < -0.01f)
            mult = downMul;

        rb.AddForce(Vector3.up * newGravity * mult, ForceMode.Acceleration);

        Vector3 v = rb.velocity;
        v.y = Mathf.Clamp(v.y, -speedLimit, float.MaxValue);
        rb.velocity = v;
    }
}