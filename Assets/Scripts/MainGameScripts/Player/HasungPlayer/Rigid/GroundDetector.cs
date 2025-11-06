using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Header("Ground Check (Sphere)")]
    [SerializeField] private Transform groundCheck;       // 발 아래 지점
    [SerializeField] private float boxX = 0.5f; // 구 반지름
    [SerializeField] private float boxZ = 0.5f; // 구 반지름
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
        // 1) BoxCast용 halfExtents 정의 (박스 크기의 반)
        Vector3 halfExtents = new Vector3(boxX * 0.5f, 0.1f, boxZ * 0.5f);

        // 2) BoxCast 시작점 (박스의 위쪽)에 조금 올려서
        float maxDistance = maxFallDistance + 0.1f;
        Vector3 origin = groundCheck.position + Vector3.up * maxDistance;

        // 3) 아래로 BoxCast 실행
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
}
