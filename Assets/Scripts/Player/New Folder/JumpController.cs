using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("점프높이(정확한 값이 아님)")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f)]
    [Tooltip("점프 최대 높이 도달시간(정확한 값이 아님)")]
    public float timeToJumpApex;
    [Tooltip("더블점프 가능상태(시스템계산용)")]
    public bool doubleJumpActive = false;
    [Tooltip("더블 점프를 가능하게 만들지 여부")]
    public bool allowDoubleJump = false;
    [Tooltip("더블점프 횟수")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f)]
    [Tooltip("상승할 때 중력")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f)]
    [Tooltip("하락할 때 중력")]
    public float downwardMovementMultiplier = 6.17f;
    [Tooltip("코요테 시간")]
    public float coyoteTime = 0.1f;
    [Tooltip("점프 버퍼")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(2f, 8f)]
    [Tooltip("점프도중 손 놓으면 얼마나 빠르게 떨어질지")]
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
