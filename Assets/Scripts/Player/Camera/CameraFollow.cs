using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private Transform player;
    private MovementController movementController;
    private PlayerManagement playerMovement;
    private Rigidbody playerRb;

    [Header("카메라 이동 범위")]
    [SerializeField] private float minX, maxX;
    [SerializeField] private float minY, maxY;

    [Header("오프셋 설정")]
    [SerializeField] private float yOffset = 3f;    // Y 오프셋
    [SerializeField] private float xLeadDist = 2f;    // 입력 시 앞서나갈 거리

    [Header("부드러움 설정")]
    [SerializeField] private float walkSmoothTime = 0.3f;   // 걷기 시 X축 smoothTime
    [SerializeField] private float runSmoothTime = 0.15f;  // 달리기 시 X축 smoothTime
    [SerializeField] private float ySmoothTime = 0.3f;   // Y축 smoothTime

    [Header("강제 이동 감지 임계값")]
    [SerializeField] private float forcedMoveThreshold = 0.01f;

    [Header("플레이어 앞선 최대 허용 거리")]
    [SerializeField] private float maxPlayerAhead = 3f;

    // 내부 상태
    private float currentX, currentY;
    private float xVelocity, yVelocity;
    private float desiredY;
    private Vector3 lastPlayerPos;
    private bool wasMovingInput;
    private bool wasGrounded;

    void Start()
    {
        if (player == null)
            player = GameManager.Instance.player.transform;

        movementController = player.GetComponent<MovementController>();
        playerMovement = player.GetComponent<PlayerManagement>();
        playerRb = player.GetComponent<Rigidbody>();

        currentX = transform.position.x;
        currentY = transform.position.y;
        desiredY = currentY;
        lastPlayerPos = player.position;
        wasMovingInput = false;
        wasGrounded = playerMovement != null && playerMovement.IsGrounded;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 p = player.position;
        bool grounded = playerMovement != null && playerMovement.IsGrounded;

        // ──────────────────────────────────────────────────
        // 1) 착지 감지: 공중 → 지면 전환 시 Y 목표 갱신
        // ──────────────────────────────────────────────────
        if (!wasGrounded && grounded)
        {
            // 한번이라도 땅에 닿았을 때, 그 위치에 yOffset을 더한 값을 목표로 저장
            desiredY = Mathf.Clamp(p.y + yOffset, minY, maxY);
            yVelocity = 0f;  // 부드러운 보간 초기화
        }

        // ──────────────────────────────────────────────────
        // 2) 입력 여부 판단 & 강제 이동(에스컬레이터) 감지
        // ──────────────────────────────────────────────────
        float input = movementController != null ? movementController.inputX : 0f;
        bool movingInput = Mathf.Abs(input) > 0.1f;

        // 수평 외부 힘만 감지하도록 deltaX만 사용
        float deltaX = p.x - lastPlayerPos.x;
        bool forcedMoveX = !movingInput && Mathf.Abs(deltaX) > forcedMoveThreshold;

        // ──────────────────────────────────────────────────
        // 3) 강제 이동 시 X/Y 즉시 스냅 고정
        // ──────────────────────────────────────────────────
        if (forcedMoveX)
        {
            // X
            currentX = Mathf.Clamp(p.x, minX, maxX);
            // Y: 착지 시점에 결정된 desiredY 그대로 스냅
            currentY = desiredY;
            xVelocity = yVelocity = 0f;

            ApplyPosition();
            UpdateState(p, movingInput, grounded);
            return;
        }

        // ──────────────────────────────────────────────────
        // 4) X축: 입력 기반 리드 + 속도 따라 동적 smoothTime + 앞서감 제한
        // ──────────────────────────────────────────────────
        float lead = movingInput ? Mathf.Sign(input) * xLeadDist : 0f;
        float targetX = Mathf.Clamp(p.x + lead, minX, maxX);

        // 달리기 속도 비율 계산
        float speedNorm = Mathf.InverseLerp(
            movementController.maxWalkSpeed,
            movementController.maxRunSpeed,
            Mathf.Abs(playerRb.velocity.x)
        );
        float xSmooth = Mathf.Lerp(walkSmoothTime, runSmoothTime, speedNorm);

        currentX = Mathf.SmoothDamp(currentX, targetX, ref xVelocity, xSmooth);

        // 플레이어가 너무 앞서 나가면 허용 최대만큼만 앞서게
        float ahead = p.x - currentX;
        if (ahead > maxPlayerAhead)
            currentX = p.x - maxPlayerAhead;

        // ──────────────────────────────────────────────────
        // 5) Y축: 착지 상태일 때만 부드럽게 보간 (공중 시 고정)
        // ──────────────────────────────────────────────────
        if (grounded)
        {
            currentY = Mathf.SmoothDamp(currentY, desiredY, ref yVelocity, ySmoothTime);
        }

        // ──────────────────────────────────────────────────
        // 6) 위치 적용 및 상태 갱신
        // ──────────────────────────────────────────────────
        ApplyPosition();
        UpdateState(p, movingInput, grounded);
    }

    private void ApplyPosition()
    {
        transform.position = new Vector3(currentX, currentY, -9);
    }

    private void UpdateState(Vector3 playerPos, bool movingInput, bool grounded)
    {
        lastPlayerPos = playerPos;
        wasMovingInput = movingInput;
        wasGrounded = grounded;
    }
}
