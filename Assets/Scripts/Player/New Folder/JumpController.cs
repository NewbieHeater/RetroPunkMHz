using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("��������(��Ȯ�� ���� �ƴ�)")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f)]
    [Tooltip("���� �ִ� ���� ���޽ð�(��Ȯ�� ���� �ƴ�)")]
    public float timeToJumpApex;
    [Tooltip("�������� ���ɻ���(�ý��۰���)")]
    public bool doubleJumpActive = false;
    [Tooltip("���� ������ �����ϰ� ������ ����")]
    public bool allowDoubleJump = false;
    [Tooltip("�������� Ƚ��")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f)]
    [Tooltip("����� �� �߷�")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)]
    [Tooltip("�϶��� �� �߷�")]
    public float downwardMovementMultiplier = 6.17f;
    [Tooltip("�ڿ��� �ð�")]
    public float coyoteTime = 0.1f;
    [Tooltip("���� ����")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(2f, 8f)]
    [Tooltip("�������� �� ������ �󸶳� ������ ��������")]
    public float jumpCutOff;

    public float coyoteTimer = 0f;

    public bool TryJump(bool jumpPressed, bool isGrounded, ref Vector3 velocity)
    {
        if (isGrounded) { coyoteTimer = coyoteTime; doubleJumpActive = false; }
        else { coyoteTimer -= Time.fixedDeltaTime; }

        if (jumpPressed && (isGrounded || coyoteTimer > 0f || doubleJumpActive))
        {
            float gravity = ((-2f * maxJumpHeight) / (timeToJumpApex * timeToJumpApex)) / 9.81f;
            float jumpSpeed = Mathf.Sqrt(-2f * gravity * maxJumpHeight);
            var currentVerticalSpeed = Vector3.Dot(velocity, Vector3.down);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

            velocity += Vector3.up * (targetVerticalSpeed + currentVerticalSpeed);

            if (!isGrounded) doubleJumpActive = false;
            coyoteTimer = 0f;

            return true;
        }
        return false;
    }
}
