using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Tooltip("���� üũ ��ġ")]
    public Transform groundCheck;
    [Tooltip("���� üũ ���� ������")]
    public float groundCheckDistance = 0.5f;
    [Tooltip("� ���̾ �������� ��������")]
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
