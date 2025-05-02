using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("이동속도")]
    public float maxSpeed = 5f;
    [Tooltip("가속")]
    public float accelerationTime = 0.1f;
    [Tooltip("감속")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("공중에서의 이동력(이동속도 x airControl)")]
    public float airControl = 0.5f;
    [Tooltip("캐릭터 회전 속도")]
    public float rotationSpeed = 0.2f;
    [Tooltip("캐릭터 매쉬")]
    public Transform playerMesh;

    public void HandleRotation(float inputX)
    {
        if (Mathf.Abs(inputX) > 0.01f)
            playerMesh.rotation = Quaternion.Slerp(
                playerMesh.rotation,
                Quaternion.LookRotation(Vector3.right * inputX),
                rotationSpeed
            );
    }

    public void ProcessMovement(float inputX, bool isGrounded, ref Vector3 velocity)
    {
        float targetVelocity = inputX * maxSpeed;
        float accelRate = isGrounded ? (maxSpeed / accelerationTime) : (maxSpeed / accelerationTime * airControl);
        float decelRate = isGrounded ? (maxSpeed / decelerationTime) : (maxSpeed / decelerationTime * airControl);

        float newX = Mathf.Abs(targetVelocity) > 0.01f
            ? Mathf.MoveTowards(velocity.x, targetVelocity, accelRate * Time.fixedDeltaTime)    //이동시 가속
            : Mathf.MoveTowards(velocity.x, 0f, decelRate * Time.fixedDeltaTime);               //이동을 멈췄을 때 감속

        velocity.x = newX;
    }
}
