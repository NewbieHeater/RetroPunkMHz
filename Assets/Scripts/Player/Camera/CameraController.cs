using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("따라갈 대상 (보통 플레이어의 Transform)")]
    public Transform target;
    [Tooltip("대상과의 초기 오프셋 (설정하지 않으면 시작 시 자동 계산)")]
    public Vector3 offset = Vector3.zero;
    [Tooltip("카메라 X축(수평) 보간 시간")]
    public float dampTimeX = 0.15f;
    [Tooltip("카메라 Y축(수직) 보간 시간")]
    public float dampTimeY = 0.15f;
    [Tooltip("대상의 이동 방향으로 선행하는 거리 (0이면 중앙)")]
    public float leadDistance = 2f;
    [Tooltip("대상의 점프 시 Y축 반응을 무시할지 여부 (true면 점프 시 Y축 업데이트를 최소화)")]
    public bool ignoreJump = true;

    [Header("Optional - Ground Detection")]
    [Tooltip("지면 체크용 Transform (대상이 지면에 닿았는지 확인)")]
    public Transform groundCheck;
    [Tooltip("지면 체크 반지름")]
    public float groundCheckRadius = 0.2f;
    [Tooltip("어떤 레이어를 지면으로 간주할지")]
    public LayerMask groundLayer;

    // 내부 보간 변수
    private Vector3 smoothVelocity;
    // 대상이 지면에 있었을 때 기록한 Y값 (점프 무시 기능에 사용)
    private float groundCameraY;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraController3D: target이 할당되지 않았습니다.");
            return;
        }

        // 오프셋이 설정되어 있지 않다면 초기 위치 차이를 이용해 계산
        if (offset == Vector3.zero)
            offset = transform.position - target.position;

        // 초기 지면 Y 위치 저장
        groundCameraY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // 기본적으로 대상의 현재 위치와 오프셋을 더한 위치가 목표 위치
        Vector3 desiredPosition = target.position + offset;

        // 대상의 이동 방향(수평 평면)으로 선행(offset) 적용
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // X, Z 평면 상의 속도만 고려
            Vector3 horizontalVelocity = new Vector3(targetRb.velocity.x, 0f, targetRb.velocity.z);
            if (horizontalVelocity.magnitude > 0.1f)
            {
                // 이동 방향 정규화 후 leadDistance 만큼 오프셋 추가
                desiredPosition += horizontalVelocity.normalized * leadDistance;
            }
        }

        // 점프 무시 기능: 대상이 점프할 경우 Y값 업데이트를 최소화
        if (ignoreJump)
        {
            bool isGrounded = false;
            // 지면 체크를 위한 groundCheck이 할당되어 있다면 OverlapSphere로 판정
            if (groundCheck != null)
            {
                isGrounded = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, groundLayer).Length > 0;
            }

            // 대상이 지면에 있으면 최신 Y값을 기록
            if (isGrounded)
            {
                groundCameraY = target.position.y;
            }
            else
            {
                // 대상의 Y가 기록된 값보다 낮아지면 업데이트 (내리막이나 착지 시 반영)
                if (target.position.y < groundCameraY)
                {
                    groundCameraY = target.position.y;
                }
            }
            // Y값은 기록된 지면 Y + 기존 오프셋의 Y를 사용
            desiredPosition.y = groundCameraY + offset.y;
        }

        // SmoothDamp를 사용하여 부드럽게 카메라 이동 (X, Y, Z 각각 보간)
        Vector3 currentPos = transform.position;
        float smoothX = Mathf.SmoothDamp(currentPos.x, desiredPosition.x, ref smoothVelocity.x, dampTimeX);
        float smoothY = Mathf.SmoothDamp(currentPos.y, desiredPosition.y, ref smoothVelocity.y, dampTimeY);
        float smoothZ = Mathf.SmoothDamp(currentPos.z, desiredPosition.z, ref smoothVelocity.z, dampTimeX);
        transform.position = new Vector3(smoothX, smoothY, smoothZ);
    }

    // Scene 뷰에서 지면 체크 영역을 시각적으로 표시합니다.
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
