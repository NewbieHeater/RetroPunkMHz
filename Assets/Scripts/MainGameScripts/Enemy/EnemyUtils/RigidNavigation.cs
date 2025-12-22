// RigidNavigation.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RigidNavigation : MonoBehaviour
{
    public enum MoveMode { Walk }

    [Header("Common")]
    [SerializeField] private float speed = 3f;
    [SerializeField] public float stoppingDistance = 0.01f;

    [Header("Custom Gravity")]
    [SerializeField] private float gravityAccel = 20f;
    public Vector3 GravityDir { get; private set; } = Vector3.down;

    [Header("Layers (붙을 수 있는 표면: 바닥+벽 모두 포함)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckStartY = 0.1f;
    [SerializeField] private float groundCheckDist = 0.7f;

    [Header("Probe Common")]
    [SerializeField] private bool useSphereCast = true;
    [SerializeField] private float probeRadius = 0f;

    [Header("Front Surface Select")]
    [SerializeField] private float frontSurfaceHeight = 0.2f;
    [SerializeField] private float frontSurfaceDist = 0.45f;

    [Header("Ledge/Corner (낭떠러지 코너 벽 찾기)")]
    [SerializeField] private float ledgeAhead = 0.18f;
    [SerializeField] private float ledgeProbeExtra = 0.10f;
    [SerializeField] private float cornerBack = 0.12f;
    [SerializeField] private float cornerProbeHeight = 0.05f;
    [SerializeField] private float cornerProbeDist = 0.45f;

    [Header("Surface Align (2.5D)")]
    [SerializeField] private Vector3 planeNormal = Vector3.forward;
    [SerializeField] private float stickSnap = 0.06f;
    [SerializeField] private float stickProbeDistance = 0.6f;
    [SerializeField] private float rotateDegPerSec = 720f;

    [Header("Align State")]
    [SerializeField] private float alignMinTime = 0.08f;
    [SerializeField] private float alignDoneAngle = 4f;
    [SerializeField] private float upSmooth = 20f;

    [Header("Detach")]
    [SerializeField] private float detachGraceTime = 0.08f;

    public bool hasPath { get; private set; }
    public bool isStopped { get; set; }
    public bool IsEnabled { get; private set; } = true;
    public bool isGrounded { get; private set; }

    private MoveMode mode = MoveMode.Walk;
    private Vector3 targetPos;
    private Rigidbody rigid;
    private Collider col;

    private bool _surfaceAttached;

    // Align state
    private bool _aligning;
    private float _alignStartTime;
    private Vector3 _alignTargetNormal;
    private Vector3 _smoothedUp;
    private Vector3 _lastMoveDir = Vector3.right;

    // "정렬은 접촉당 1회" 래치
    private bool _alignLatched;
    private float _noSurfaceTime;

    public bool IsAligningToSurface => _aligning;
    public bool SurfaceAttached => _surfaceAttached;
    public bool AlignLatched => _alignLatched;

    // External motion gate (knockback, etc.)
    private float _externalMotionTime;

    private float _bodyExtent = 0.25f;

    private void Awake()
    {
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
        if (wallLayer == 0) wallLayer = groundLayer;

        rigid = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rigid.useGravity = false;

        hasPath = false;
        isStopped = false;
        mode = MoveMode.Walk;
        targetPos = transform.position;

        SetGravity(Vector3.down);

        if (planeNormal.sqrMagnitude < 1e-6f) planeNormal = Vector3.forward;
        planeNormal = planeNormal.normalized;

        CacheBodyExtent();
        AutoProbeRadius();

        _smoothedUp = transform.up;

        Vector3 initFwd = transform.right;
        if (Vector3.ProjectOnPlane(transform.forward, Vector3.up).sqrMagnitude > 1e-6f)
            initFwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _lastMoveDir = initFwd;
    }

    private void CacheBodyExtent()
    {
        if (!col)
        {
            _bodyExtent = 0.25f;
            return;
        }

        Bounds b = col.bounds;
        _bodyExtent = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));
        if (_bodyExtent < 0.01f) _bodyExtent = 0.25f;
    }

    private void AutoProbeRadius()
    {
        if (probeRadius > 0f) return;

        if (col is CapsuleCollider cap)
        {
            probeRadius = Mathf.Max(0.02f, cap.radius * 0.9f);
            return;
        }

        if (col is SphereCollider sph)
        {
            probeRadius = Mathf.Max(0.02f, sph.radius * 0.9f);
            return;
        }

        probeRadius = Mathf.Clamp(_bodyExtent * 0.25f, 0.03f, 0.25f);
    }

    public void SetEnabled(bool on)
    {
        if (IsEnabled == on) return;
        IsEnabled = on;
        if (!on) ResetPath();
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;

    public void SetGravity(Vector3 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.down;
        GravityDir = dir.normalized;
    }

    public void ApplyExternalVelocity(Vector3 velocity, float duration)
    {
        _externalMotionTime = Mathf.Max(_externalMotionTime, duration);
        rigid.velocity = velocity;
    }

    public void SetDestination(Vector3 dst)
    {
        mode = MoveMode.Walk;
        targetPos = dst;

        _externalMotionTime = 0f;

        if (_surfaceAttached) SetGravity(-transform.up);
        else SetGravity(Vector3.down);

        hasPath = true;
        isStopped = false;
    }

    public float RemainingDistance() => (transform.position - targetPos).magnitude;
    public float RemainingDistanceX() => Mathf.Abs(transform.position.x - targetPos.x);

    public void ResetPath()
    {
        hasPath = false;
        _aligning = false;
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * groundCheckStartY,
            Vector3.down,
            out _,
            groundCheckDist,
            groundLayer,
            QueryTriggerInteraction.Ignore);

        if (!hasPath || isStopped) return;

        if (mode == MoveMode.Walk)
        {
            if (RemainingDistance() <= stoppingDistance && isGrounded)
                ResetPath();
        }
    }

    private void FixedUpdate()
    {
        if (!IsEnabled) return;

        // 중력은 항상 적용
        rigid.AddForce(GravityDir * gravityAccel, ForceMode.Acceleration);

        // 경로 없거나 멈춤
        if (!hasPath || isStopped)
        {
            if (_externalMotionTime > 0f)
            {
                _externalMotionTime -= Time.fixedDeltaTime;
                return;
            }

            Vector3 v = rigid.velocity;
            Vector3 vg = Vector3.Project(v, GravityDir);
            rigid.velocity = vg;
            return;
        }

        // 정렬 단계: 이동 금지, 회전/스냅만
        if (_aligning)
        {
            TickAlignOnly();
            return;
        }

        DoWalkOnTangent();
    }

    /// <summary>
    /// 앞에 표면이 있으면 그 normal을 반환한다.
    /// - 정면에 벽/면이 있으면 우선 그 면
    /// - 정면 벽이 없고, "앞 발밑 표면이 없으면(낭떠러지)" 코너의 수직면을 찾아 반환
    /// - 둘 다 아니면 false
    /// </summary>
    public bool TryGetAheadSurfaceNormal(out Vector3 surfaceNormal)
    {
        surfaceNormal = default;

        Vector3 moveDir = (_lastMoveDir.sqrMagnitude > 1e-6f) ? _lastMoveDir.normalized : transform.right;

        // 1) 정면 표면(벽/면) 우선
        Vector3 origin = rigid.position + transform.up * frontSurfaceHeight;

        if (useSphereCast)
        {
            if (Physics.SphereCast(origin, probeRadius, moveDir, out RaycastHit hit, frontSurfaceDist, wallLayer, QueryTriggerInteraction.Ignore))
            {
                surfaceNormal = hit.normal.normalized;
                return true;
            }
        }
        else
        {
            if (Physics.Raycast(origin, moveDir, out RaycastHit hit, frontSurfaceDist, wallLayer, QueryTriggerInteraction.Ignore))
            {
                surfaceNormal = hit.normal.normalized;
                return true;
            }
        }

        // 2) 낭떠러지 검사: 앞으로 한 걸음 위치에서 발밑(-up)에 표면이 없으면 코너 벽을 탐색
        Vector3 up = transform.up;
        Vector3 down = (-up).normalized;

        Vector3 footOrigin = rigid.position + moveDir * ledgeAhead + up * (0.02f + stickSnap);

        bool hasSurfaceBelow;
        if (useSphereCast)
        {
            hasSurfaceBelow = Physics.SphereCast(
                footOrigin, probeRadius, down, out _,
                stickProbeDistance + ledgeProbeExtra, wallLayer, QueryTriggerInteraction.Ignore);
        }
        else
        {
            hasSurfaceBelow = Physics.Raycast(
                footOrigin, down, out _,
                stickProbeDistance + ledgeProbeExtra, wallLayer, QueryTriggerInteraction.Ignore);
        }

        if (hasSurfaceBelow) return false;

        // 3) 코너 벽(수직면) 찾기: 앞/약간 위에서 뒤로 긁기
        Vector3 cornerOrigin = rigid.position + moveDir * (ledgeAhead + cornerBack) + up * cornerProbeHeight;
        RaycastHit wallHit;
        bool hitWall;
        if (useSphereCast)
        {
            hitWall = Physics.SphereCast(
                cornerOrigin, probeRadius, -moveDir, out wallHit,
                cornerProbeDist, wallLayer, QueryTriggerInteraction.Ignore);
        }
        else
        {
            hitWall = Physics.Raycast(
                cornerOrigin, -moveDir, out wallHit,
                cornerProbeDist, wallLayer, QueryTriggerInteraction.Ignore);
        }

        if (!hitWall) return false;
        surfaceNormal = wallHit.normal.normalized;

        return true;
    }

    /// <summary>
    /// 표면에 붙기 위한 정렬 시작(접촉당 1회만)
    /// </summary>
    public void StartSurfaceAlign(Vector3 surfaceNormal)
    {
        if (_alignLatched) return;
        if (_aligning) return;

        _alignLatched = true;

        _aligning = true;
        _alignStartTime = Time.time;
        _alignTargetNormal = (surfaceNormal.sqrMagnitude < 1e-6f) ? transform.up : surfaceNormal.normalized;

        _surfaceAttached = true;
        _noSurfaceTime = 0f;
    }

    private void TickAlignOnly()
    {
        // 1) up 스무딩(목표 normal로)
        _smoothedUp = Vector3.Slerp(
            _smoothedUp,
            _alignTargetNormal,
            1f - Mathf.Exp(-upSmooth * Time.fixedDeltaTime));

        Vector3 up = _smoothedUp.normalized;

        // 2) "벽을 타는 진행 방향"은 월드 Up을 현재 표면에 투영한 방향을 기본으로 한다.
        //    좌/우 벽에서 Cross 부호가 뒤집혀도, 이 방식은 기본 진행이 일관된다.
        Vector3 climbFwd = Vector3.ProjectOnPlane(Vector3.up, up);
        if (climbFwd.sqrMagnitude < 1e-6f)
        {
            // 예외 대비(거의 발생하지 않음)
            Vector3 pn = planeNormal;
            if (Mathf.Abs(Vector3.Dot(up, pn)) > 0.95f)
                pn = (transform.right.sqrMagnitude > 1e-6f) ? transform.right.normalized : Vector3.right;

            climbFwd = Vector3.Cross(pn, up);
        }
        climbFwd.Normalize();

        // 항상 월드 +Y 쪽으로 향하도록 보정(붙자마자 아래로 향하는 문제 방지)
        if (Vector3.Dot(climbFwd, Vector3.up) < 0f)
            climbFwd = -climbFwd;

        Quaternion targetRot = Quaternion.LookRotation(climbFwd, up);
        rigid.MoveRotation(Quaternion.RotateTowards(rigid.rotation, targetRot, rotateDegPerSec * Time.fixedDeltaTime));

        // 이후 이동에서 참조되므로 갱신
        _lastMoveDir = climbFwd;

        // 3) 중력/스냅
        SetGravity(-transform.up);
        StickToSurface();

        // 4) 완료 판정
        float ang = Vector3.Angle(transform.up, _alignTargetNormal);
        bool minTimeOk = (Time.time - _alignStartTime) >= alignMinTime;
        bool angleOk = ang <= alignDoneAngle;

        if (minTimeOk && angleOk)
            _aligning = false;
    }

    private void StickToSurface()
    {
        Vector3 origin = rigid.position + transform.up * (0.02f + stickSnap);
        Vector3 down = (-transform.up).normalized;

        bool ok;
        RaycastHit hit;

        if (useSphereCast)
        {
            ok = Physics.SphereCast(origin, probeRadius, down, out hit, stickProbeDistance, wallLayer, QueryTriggerInteraction.Ignore);
        }
        else
        {
            ok = Physics.Raycast(origin, down, out hit, stickProbeDistance, wallLayer, QueryTriggerInteraction.Ignore);
        }

        if (!ok)
        {
            _noSurfaceTime += Time.fixedDeltaTime;

            // 일정 시간 이상 표면을 못 찾으면 "떨어짐"으로 간주하고 래치 해제
            if (_noSurfaceTime >= detachGraceTime)
            {
                _surfaceAttached = false;
                _alignLatched = false;
            }
            return;
        }

        _noSurfaceTime = 0f;

        Vector3 target = hit.point + hit.normal.normalized * stickSnap;
        rigid.MovePosition(Vector3.Lerp(rigid.position, target, 20f * Time.fixedDeltaTime));
        _surfaceAttached = true;
    }

    private void DoWalkOnTangent()
    {
        Vector3 to = targetPos - transform.position;

        // 현재 중력 방향에 대해 접선 방향으로만 이동
        Vector3 tangent = Vector3.ProjectOnPlane(to, GravityDir);
        if (tangent.sqrMagnitude < 1e-8f)
            tangent = Vector3.ProjectOnPlane(transform.right, GravityDir);

        Vector3 dir = tangent.normalized;

        // 진행 방향 캐시(표면 선택/정렬에서 기준으로 사용)
        _lastMoveDir = dir;

        Vector3 step = dir * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + step);

        // 이동 중에는 진행 방향만 약하게 맞춤
        if (dir.sqrMagnitude > 1e-6f)
        {
            Quaternion look = Quaternion.LookRotation(dir, transform.up);
            rigid.MoveRotation(Quaternion.RotateTowards(rigid.rotation, look, rotateDegPerSec * Time.fixedDeltaTime));
        }

        // 붙어있는 동안은 중력/스냅 유지
        if (_surfaceAttached)
        {
            SetGravity(-transform.up);
            StickToSurface();
        }
        else
        {
            // 바닥 복귀 상태에서는 기본 중력
            SetGravity(Vector3.down);
        }
    }
}
