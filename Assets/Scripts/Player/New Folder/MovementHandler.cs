using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementHandler
{
    private Rigidbody rb;
    private float maxSpeed, accelTime, decelTime, airControl, rotationSpeed;
    private Transform meshTransform;

    public MovementHandler(Rigidbody rb, float maxSpeed, float accelTime, float decelTime, float airControl, float rotationSpeed, Transform mesh)
    {
        this.rb = rb;
        this.maxSpeed = maxSpeed;
        this.accelTime = accelTime;
        this.decelTime = decelTime;
        this.airControl = airControl;
        this.rotationSpeed = rotationSpeed;
        this.meshTransform = mesh;
    }

    public void HandleMovement(float inputX, bool isGrounded)
    {
        float targetVel = inputX * maxSpeed;
        float accel = isGrounded ? maxSpeed / accelTime : maxSpeed / accelTime * airControl;
        float decel = isGrounded ? maxSpeed / decelTime : maxSpeed / decelTime * airControl;

        float newX = Mathf.Abs(targetVel) > 0.01f
            ? Mathf.MoveTowards(rb.velocity.x, targetVel, accel * Time.fixedDeltaTime)
            : Mathf.MoveTowards(rb.velocity.x, 0f, decel * Time.fixedDeltaTime);

        rb.velocity = new Vector3(newX, rb.velocity.y, 0f);

        // Rotation
        if (Mathf.Abs(newX) > 0.01f && meshTransform != null)
            meshTransform.rotation = Quaternion.Slerp(
                meshTransform.rotation,
                Quaternion.LookRotation(Vector3.right * newX),
                rotationSpeed
            );
    }
}