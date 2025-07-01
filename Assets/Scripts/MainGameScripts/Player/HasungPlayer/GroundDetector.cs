using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground Check (Sphere)")]
    [SerializeField] private Transform groundCheck;       // �� �Ʒ� ����
    [SerializeField] private float boxX = 0.5f; // �� ������
    [SerializeField] private float boxZ = 0.5f; // �� ������
    [SerializeField] private float maxFallDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public RaycastHit LastHit { get; private set; }

    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public float checkRadius = 0.5f;
    public float checkDistance = 0.3f;

    public void UpdateGroundStatus()
    {
        // 1) BoxCast�� halfExtents ���� (�ڽ� ũ���� ��)
        Vector3 halfExtents = new Vector3(boxX * 0.5f, 0.1f, boxZ * 0.5f);

        // 2) BoxCast ������ (�ڽ��� ����)�� ���� �÷���
        float maxDistance = maxFallDistance + 0.1f;
        Vector3 origin = groundCheck.position + Vector3.up * maxDistance;

        // 3) �Ʒ��� BoxCast ����
        if (Physics.BoxCast(
                origin,
                halfExtents,
                Vector3.down,
                out RaycastHit hit,
                Quaternion.identity,
                maxDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            IsGrounded = true;
            LastHit = hit;
            animator.SetBool("Grounded", true);
        }
        else
        {
            IsGrounded = false;
            LastHit = default;
            animator.SetBool("Grounded", false);
        }
    }


    //public void UpdateGroundStatus()
    //{
    //    Vector3 boxSize = new Vector3(boxX*0.5f, 0.1f, boxZ * 0.5f);
    //    bool grounded = Physics.CheckBox(groundCheck.position, boxSize, Quaternion.identity, groundLayer);

    //    IsGrounded = grounded;
    //    animator.SetBool("Grounded", grounded);

    //    // 2) �������, ��Ȯ�� �浹 ����(RaycastHit)�� ���
    //    if (grounded)
    //    {
    //        // �� �˻�� ��ġ���� �ణ ���� �÷��� �� �������� ����ĳ��Ʈ
    //        Vector3 rayOrigin = groundCheck.position + Vector3.up * 0.1f;
    //        float rayDistance = maxFallDistance + 0.1f;

    //        if (Physics.Raycast(
    //            rayOrigin,
    //            Vector3.down,
    //            out RaycastHit hit,
    //            rayDistance,
    //            groundLayer,
    //            QueryTriggerInteraction.Ignore))
    //        {
    //            LastHit = hit;
    //            return;
    //        }
    //    }

    //    // �ƹ��͵� �� �¾����� �⺻��
    //    LastHit = default;
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 boxSize = new Vector3(boxX, 0.2f, boxZ);
        Gizmos.DrawWireCube(groundCheck.position, boxSize);
    }
}
