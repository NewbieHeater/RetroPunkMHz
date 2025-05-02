using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    public float defaultGravityScale = 1f;
    public float upwardMultiplier = 1f;
    public float downwardMultiplier = 6.17f;
    public float speedLimit = 50f;
    public float maxJumpHeight = 4f;
    public float timeToJumpApex = 0.4f;

    public void Init(float maxJumpHeight, float timeToJumpApex)
    {
        this.maxJumpHeight = maxJumpHeight;
        this.timeToJumpApex = timeToJumpApex;
    }

    public void ApplyGravity(Rigidbody rb, bool isGrounded)
    {
        
        float gravity = ((-2f * maxJumpHeight) / (timeToJumpApex * timeToJumpApex)) / 9.81f;
        float multiplier;
        if (isGrounded)
        {
            multiplier = defaultGravityScale;
        }
        else
        {
            multiplier = rb.velocity.y > 0f ? upwardMultiplier : downwardMultiplier;
        }
        
        rb.AddForce(Vector3.up * gravity * multiplier, ForceMode.Acceleration);
        Debug.Log(Vector3.up * gravity * multiplier);
    }

    public void ClampFallVelocity(Rigidbody rb)
    {
        var v = rb.velocity;
        v.y = Mathf.Clamp(v.y, -speedLimit, float.MaxValue);
        rb.velocity = v;
    }
}
