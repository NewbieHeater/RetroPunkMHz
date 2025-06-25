using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("���� ��� (���� �÷��̾��� Transform)")]
    public Transform target;
    [Tooltip("������ �ʱ� ������ (�������� ������ ���� �� �ڵ� ���)")]
    public Vector3 offset = Vector3.zero;
    [Tooltip("ī�޶� X��(����) ���� �ð�")]
    public float dampTimeX = 0.15f;
    [Tooltip("ī�޶� Y��(����) ���� �ð�")]
    public float dampTimeY = 0.15f;
    [Tooltip("����� �̵� �������� �����ϴ� �Ÿ� (0�̸� �߾�)")]
    public float leadDistance = 2f;
    [Tooltip("����� ���� �� Y�� ������ �������� ���� (true�� ���� �� Y�� ������Ʈ�� �ּ�ȭ)")]
    public bool ignoreJump = true;

    [Header("Optional - Ground Detection")]
    [Tooltip("���� üũ�� Transform (����� ���鿡 ��Ҵ��� Ȯ��)")]
    public Transform groundCheck;
    [Tooltip("���� üũ ������")]
    public float groundCheckRadius = 0.2f;
    [Tooltip("� ���̾ �������� ��������")]
    public LayerMask groundLayer;

    // ���� ���� ����
    private Vector3 smoothVelocity;
    // ����� ���鿡 �־��� �� ����� Y�� (���� ���� ��ɿ� ���)
    private float groundCameraY;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraController3D: target�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // �������� �����Ǿ� ���� �ʴٸ� �ʱ� ��ġ ���̸� �̿��� ���
        if (offset == Vector3.zero)
            offset = transform.position - target.position;

        // �ʱ� ���� Y ��ġ ����
        groundCameraY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // �⺻������ ����� ���� ��ġ�� �������� ���� ��ġ�� ��ǥ ��ġ
        Vector3 desiredPosition = target.position + offset;

        // ����� �̵� ����(���� ���)���� ����(offset) ����
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // X, Z ��� ���� �ӵ��� ���
            Vector3 horizontalVelocity = new Vector3(targetRb.velocity.x, 0f, targetRb.velocity.z);
            if (horizontalVelocity.magnitude > 0.1f)
            {
                // �̵� ���� ����ȭ �� leadDistance ��ŭ ������ �߰�
                desiredPosition += horizontalVelocity.normalized * leadDistance;
            }
        }

        // ���� ���� ���: ����� ������ ��� Y�� ������Ʈ�� �ּ�ȭ
        if (ignoreJump)
        {
            bool isGrounded = false;
            // ���� üũ�� ���� groundCheck�� �Ҵ�Ǿ� �ִٸ� OverlapSphere�� ����
            if (groundCheck != null)
            {
                isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayer).Length > 0;
            }

            // ����� ���鿡 ������ �ֽ� Y���� ���
            if (isGrounded)
            {
                groundCameraY = target.position.y;
            }
            else
            {
                // ����� Y�� ��ϵ� ������ �������� ������Ʈ (�������̳� ���� �� �ݿ�)
                if (target.position.y < groundCameraY)
                {
                    groundCameraY = target.position.y;
                }
            }
            // Y���� ��ϵ� ���� Y + ���� �������� Y�� ���
            desiredPosition.y = groundCameraY + offset.y;
        }

        // SmoothDamp�� ����Ͽ� �ε巴�� ī�޶� �̵� (X, Y, Z ���� ����)
        Vector3 currentPos = transform.position;
        float smoothX = Mathf.SmoothDamp(currentPos.x, desiredPosition.x, ref smoothVelocity.x, dampTimeX);
        float smoothY = Mathf.SmoothDamp(currentPos.y, desiredPosition.y, ref smoothVelocity.y, dampTimeY);
        float smoothZ = Mathf.SmoothDamp(currentPos.z, desiredPosition.z, ref smoothVelocity.z, dampTimeX);
        transform.position = new Vector3(smoothX, smoothY, smoothZ);
    }

    // Scene �信�� ���� üũ ������ �ð������� ǥ���մϴ�.
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
