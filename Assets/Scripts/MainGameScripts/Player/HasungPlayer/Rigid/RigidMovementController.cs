using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class RigidMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxWalkSpeed = 5f;
    public float maxRunSpeed = 8f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.2f;
    [Range(0f, 1f)] public float airControl = 0.5f;
    public float rotationSpeed = 0.2f;

    [Header("Glitch Pass")]
    [SerializeField] private float _teleportSpace = 0.5f;      // 통과 허용 두께(X축 기준)
    [SerializeField] private float _postTeleportOffset = 1.2f; // 반대편으로 얼마나 더 밀어낼지
    [SerializeField] private float _teleportCooldown = 0.08f;  // 같은 벽에 대한 재텔레포트 쿨다운
    private float _lastTeleportTime = -999f;
    private int _lastGlitchId = -1;

    private Rigidbody rb;
    private Animator animator;
    private GroundDetector groundDetector;

    public float inputX;
    private bool wasOnSlope;
    public bool isOnSlope;

    // 의도: LeftShift로 달리기 토글 유지 / 통과권 비소모 유지
    private bool _isSpeedup = false; // 달리기 모드
    private bool _canPass = false; // Glitch 통과권(소모하지 않음)

    public Vector3 newVelocity;

    public void Initialize(GroundDetector gd)
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        groundDetector = gd;
    }

    public void HandleInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // Shift 눌렀을 때: 달리기 ON + 통과권 부여(비소모)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isSpeedup = true;
            _canPass = true;
        }
    }

    // --- 외부에서 FixedUpdate에서 호출 ---
    public void ProcessMovement(bool isGrounded, RaycastHit groundHit, float dt)
    {
        SnapStopIfNeeded(isGrounded);

        if (_isSpeedup) RunMove(isGrounded, groundHit, dt);
        else WalkMove(isGrounded, groundHit, dt);

        ApplyFacing(dt);
    }

    private void WalkMove(bool isGrounded, RaycastHit groundHit, float dt)
    {
        ApplyLocomotion(
            maxSpeed: maxWalkSpeed,
            accelTime: accelerationTime,
            decelTime: decelerationTime,
            isGrounded: isGrounded,
            groundHit: groundHit,
            dt: dt
        );
    }

    private void RunMove(bool isGrounded, RaycastHit groundHit, float dt)
    {
        // 필요하면 달리기 전용 accel/decel/airControl로 분기 가능
        ApplyLocomotion(
            maxSpeed: maxRunSpeed,
            accelTime: accelerationTime,
            decelTime: decelerationTime,
            isGrounded: isGrounded,
            groundHit: groundHit,
            dt: dt
        );
    }

    private void ApplyLocomotion(float maxSpeed, float accelTime, float decelTime,
                                 bool isGrounded, RaycastHit groundHit, float dt)
    {
        float targetVx = inputX * maxSpeed;
        float accel = isGrounded ? (maxSpeed / accelTime) : (maxSpeed / accelTime) * airControl;
        float decel = isGrounded ? (maxSpeed / decelTime) : (maxSpeed / decelTime) * airControl;

        float newVx = (Mathf.Abs(inputX) > 0.01f)
            ? Mathf.MoveTowards(rb.velocity.x, targetVx, accel * dt)
            : Mathf.MoveTowards(rb.velocity.x, 0f, decel * dt);

        isOnSlope = IsOnSlope(groundHit) && isGrounded;

        if (isOnSlope)
        {
            // 슬로프 분모 보호 + y 보존
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.right, groundHit.normal).normalized;
            float eps = 1e-4f;
            float denom = Mathf.Abs(slopeDir.x) < eps ? (slopeDir.x >= 0 ? eps : -eps) : slopeDir.x;

            newVelocity = slopeDir * (newVx / denom);
            newVelocity.y = rb.velocity.y; // y 보존
        }
        else
        {
            // 지상이라도 y 보존(필요 시 StickToGround 별도 구현 권장)
            newVelocity = new Vector3(newVx, rb.velocity.y, 0f);
        }

        wasOnSlope = isOnSlope;
        rb.velocity = newVelocity;
    }

    private void SnapStopIfNeeded(bool isGrounded)
    {
        // 입력 없고 지상이며 수평속도 거의 0이면 x만 0으로 스냅( y는 보존 )
        if (Mathf.Approximately(inputX, 0f) && isGrounded && Mathf.Abs(rb.velocity.x) < 0.0005f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    private void ApplyFacing(float dt)
    {
        if (Mathf.Abs(inputX) > 0.01f)
        {
            Vector3 dir = new Vector3(inputX, 0, 0);
            Quaternion targetRot = Quaternion.LookRotation(dir);
            animator.transform.rotation = Quaternion.Slerp(
                animator.transform.rotation, targetRot, rotationSpeed * dt);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Glitch"))
        {
            if (!_canPass) return; // 통과권 없으면 무시(통과권은 소모하지 않음)

            Collider wallCol = collision.collider;
            int id = wallCol.GetInstanceID();

            // 같은 벽에서 너무 빠른 재텔레포트 방지
            if (id == _lastGlitchId && Time.time - _lastTeleportTime < _teleportCooldown)
                return;

            Bounds wallBounds = wallCol.bounds;
            float width = wallBounds.size.x;

            if (width < _teleportSpace)
            {
                float playerX = transform.position.x;
                float minX = wallBounds.min.x;
                float maxX = wallBounds.max.x;

                // 가까운 쪽에서 먼 쪽으로 순간이동
                float targetX = (Mathf.Abs(playerX - minX) < Mathf.Abs(playerX - maxX)) ? maxX : minX;

                Vector3 teleportPos = new Vector3(
                    targetX + ((playerX < targetX) ? _postTeleportOffset : -_postTeleportOffset),
                    transform.position.y,
                    transform.position.z
                );

                // Rigidbody로 이동하는 것이 더 안전
                rb.position = teleportPos;

                // 통과권은 유지하되, 같은 벽 재충돌로 인한 연속 텔레포트만 제한
                _lastGlitchId = id;
                _lastTeleportTime = Time.time;
            }
        }
        else if (!collision.gameObject.CompareTag("Ground"))
        {
            // 사용자 의도: 다른 오브젝트와 충돌 시 러닝/통과권 해제
            _isSpeedup = false;
            _canPass = false;
        }
    }

    public void UpdateAnimationStates()
    {
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        bool moving = Mathf.Abs(inputX) > 0.01f;

        animator.SetBool("Move", moving);
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("Idle", !moving);
        // 필요 시 animator.SetFloat("Sprint", _isSpeedup ? 1f : 0f);
    }

    private bool IsOnSlope(RaycastHit hit)
    {
        if (hit.collider == null) return false;
        float angle = Vector3.Angle(Vector3.up, hit.normal);
        return angle > 0f && angle < 55f;
    }

    public void ForceStop()
    {
        rb.velocity = Vector3.zero;
        UpdateAnimationStates();
    }
}
