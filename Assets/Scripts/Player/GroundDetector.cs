using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Tooltip("지면 체크 위치")]
    public Transform groundCheck;
    [Tooltip("지면 체크 길이 반지름")]
    public float groundCheckDistance = 0.5f;
    [Tooltip("어떤 레이어를 지면으로 간주할지")]
    public LayerMask groundLayer;

    public bool isGrounded { get; private set; }

    public void CheckGround()
    {
        isGrounded = Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * groundCheckDistance
        );
    }
}
