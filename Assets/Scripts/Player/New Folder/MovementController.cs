using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("�̵��ӵ�")]
    public float maxSpeed = 5f;
    [Tooltip("����")]
    public float accelerationTime = 0.1f;
    [Tooltip("����")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("���߿����� �̵���(�̵��ӵ� x airControl)")]
    public float airControl = 0.5f;
    [Tooltip("ĳ���� ȸ�� �ӵ�")]
    public float rotationSpeed = 0.2f;
    [Tooltip("ĳ���� �Ž�")]
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
            ? Mathf.MoveTowards(velocity.x, targetVelocity, accelRate * Time.fixedDeltaTime)    //�̵��� ����
            : Mathf.MoveTowards(velocity.x, 0f, decelRate * Time.fixedDeltaTime);               //�̵��� ������ �� ����

        velocity.x = newX;
    }
}
