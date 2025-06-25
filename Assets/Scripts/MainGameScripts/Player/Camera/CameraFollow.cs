using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private Transform player;
    private MovementController movementController;
    private PlayerManagement playerMovement;
    private Rigidbody playerRb;

    [Header("로컬 오프셋 범위")]
    [SerializeField] private float minX = -3f, maxX = 3f;
    [SerializeField] private float minY = -2f, maxY = 5f;   // 낙하 시 아래로 내려오기 위해 minY를 음수로

    [Header("오프셋 및 스무스")]
    [SerializeField] private float yOffset = 3f;     // 착지 시 카메라 높이 오프셋
    [SerializeField] private float xLeadDist = 2f;     // 좌우 이동 시 앞서나갈 거리
    [SerializeField] private float walkSmoothTime = 0.3f;
    [SerializeField] private float runSmoothTime = 0.15f;
    [SerializeField] private float ySmoothTime = 0.3f;

    [Header("낙하 시 추가 오프셋")]
    [SerializeField] private float fallOffsetY = 1f;   // 낙하 중 카메라를 더 아래로 내릴 거리

    // 내부 상태
    private bool wasGrounded;
    private float groundY;        // 마지막 착지 높이(월드 Y)
    private float currentX, currentY;
    private Vector2 velocity;     // SmoothDamp용 벡터 (x, y)
    private bool Smashing = false;

    void Start()
    {
        if (player == null)
            player = GameManager.Instance.player.transform;

        movementController  =   player.GetComponent<MovementController>();
        playerMovement      =   player.GetComponent<PlayerManagement>();
        playerRb            =   player.GetComponent<Rigidbody>();

        wasGrounded =   playerMovement.IsGrounded;
        groundY     =   player.position.y;
        currentX    =   transform.localPosition.x;
        currentY    =   transform.localPosition.y;
    }

    void LateUpdate()
    {
        if (Smashing)
        {
            ActivateSmashSystem();
            return;
        }
        #region ModifyCamera
        bool grounded   = playerMovement.IsGrounded;
        float input     = movementController.inputX;
        float vertVel   = playerRb.velocity.y;
        bool isRising   = !grounded && vertVel > 0f;
        bool isFalling  = !grounded && vertVel < 0f;

        // — 착지 직후
        if (!wasGrounded && grounded)
        {
            groundY = player.position.y;
            velocity = Vector2.zero;
        }
        wasGrounded = grounded;

        // 1) 로컬 X 목표
        float lead = Mathf.Abs(input) > 0.1f
                          ? Mathf.Sign(input) * xLeadDist
                          : 0f;
        float targetX = Mathf.Clamp(lead, minX, maxX);

        // 2) 로컬 Y 목표
        float rawY;
        if (grounded)
        {
            rawY = yOffset;   // 착지 중에는 항상 yOffset
        }
        else if (isRising)
        {
            rawY = yOffset - fallOffsetY;
            // 상승 중: 착지 높이에서 올라간 만큼 깎아줌
            //rawY = (groundY + yOffset) - player.position.y;
        }
        else if (isFalling)
        {
            // 낙하 중: 더 아래를 보여주기 위해 fallOffsetY만큼 내려감
            rawY = yOffset - fallOffsetY;
        }
        else
        {
            // 혹시 모를 중간 상태
            rawY = currentY;
        }
        float targetY = Mathf.Clamp(rawY, minY, maxY);

        // 3) X축 스무스 타임 (걷기↔달리기 속도 비율)
        float speedNorm = Mathf.InverseLerp(
                              movementController.maxWalkSpeed,
                              movementController.maxRunSpeed,
                              Mathf.Abs(playerRb.velocity.x)
                          );
        float xSmooth = Mathf.Lerp(walkSmoothTime, runSmoothTime, speedNorm);

        // 4) Y축 스무스 타임: 착지 중엔 ySmoothTime, 그 외엔 xSmooth
        float ySmooth = grounded ? ySmoothTime : xSmooth;

        // 5) SmoothDamp 적용
        currentX = Mathf.SmoothDamp(
                       currentX, targetX,
                       ref velocity.x, xSmooth
                   );
        currentY = Mathf.SmoothDamp(
                       currentY, targetY,
                       ref velocity.y, ySmooth
                   );

        // 6) 로컬 위치 갱신
        transform.localPosition = new Vector3(
            currentX, currentY, transform.localPosition.z
        );
        #endregion
    }

    private void ActivateSmashSystem()
    {

    }
}
