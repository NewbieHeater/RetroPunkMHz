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


    //public void UpdateGroundStatus()
    //{
    //    Vector3 boxSize = new Vector3(boxX*0.5f, 0.1f, boxZ * 0.5f);
    //    bool grounded = Physics.CheckBox(groundCheck.position, boxSize, Quaternion.identity, groundLayer);

    //    IsGrounded = grounded;
    //    animator.SetBool("Grounded", grounded);

    //    // 2) 닿았으면, 정확한 충돌 정보(RaycastHit)를 얻기
    //    if (grounded)
    //    {
    //        // 구 검사용 위치에서 약간 위로 올려서 땅 방향으로 레이캐스트
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

    //    // 아무것도 안 맞았으면 기본값
    //    LastHit = default;
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 boxSize = new Vector3(boxX, 0.2f, boxZ);
        Gizmos.DrawWireCube(groundCheck.position, boxSize);
    }
}
