using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public sealed class SurfaceWalker2_5D : MonoBehaviour
{
    private enum Mode { Walk, Climb, DownClimb }

    [Header("Motion")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float gravityAccel = 25f;
    [SerializeField] private float rotateDegPerSec = 720f;
    [SerializeField] private float maxWalkSlopeDeg = 55f;
    [SerializeField] private float wallMinSteepDeg = 70f;
    [SerializeField] private float surfaceSnapDistance = 0.25f;

    [Header("2.5D Axis")]
    [SerializeField] private bool lockZ = true;
    [SerializeField] private float zValue = 0f;

    [Header("Probes")]
    [SerializeField] private float probeRadius = 0.18f;
    [SerializeField] private float footProbeOffset = 0.15f;
    [SerializeField] private float forwardProbeOffset = 0.10f;

    [Header("Edge Probe (ledge/cliff)")]
    [SerializeField] private float edgeForward = 0.28f;
    [SerializeField] private float edgeDown = 0.45f;

    [Header("Collision Layer")]
    public LayerMask layer;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;

    [Header("Rotate Lock")]
    [SerializeField] private float angleThresholdDeg = 0.5f;
    [SerializeField] private bool stopMoveWhileRotating = true;

    [Header("Patrol (2 points ping-pong)")]
    [SerializeField] private bool useTwoPointPatrol = true;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float arriveEps = 0.08f;
    [SerializeField] private float dwellTime = 0.0f;
    [SerializeField] private bool startTowardB = true;

    [Header("Stun")]
    [Tooltip("스턴 중 upright(월드 up)로 돌아가는 회전 속도 배수")]
    [SerializeField] private float stunUprightRotateMul = 1.0f;

    [Tooltip("스턴 중 '바닥 접지' 판정 거리")]
    [SerializeField] private float stunGroundCheckDist = 0.35f;

    private Rigidbody _rb;
    private Collider _col;

    private Mode _mode = Mode.Walk;

    // 진행 방향(+1 / -1)
    private int _dirSign = 1;

    // ===== 회전 요청을 "커밋"하기 위한 상태 =====
    private bool isRotating = false;
    private Quaternion _rotTarget;
    private Mode _pendingModeAfterRotate;
    private bool _hasPendingMode = false;
    private Vector3 _lockedForward;

    // ===== 정지/재개 =====
    private bool _stopped = false;
    private bool _waiting = false;
    private float _tWaitStart = 0f;

    // ===== 2점 핑퐁 상태 =====
    private Transform _target;
    private bool _goingToB;

    // ===== 스턴 =====
    private bool _stunned = false;
    private float _stunHoldSeconds = 1f;     // "바닥 닿은 후 유지" 시간 (최소 1초 반영됨)
    private bool _stunGroundedOnce = false;  // 스턴 중 바닥을 한 번이라도 밟았는지
    private float _tStunGrounded = 0f;       // 바닥을 밟은 시점

    public bool IsStopped => _stopped;
    public bool IsStunned => _stunned;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        InitTwoPointPatrol();
    }

    private void InitTwoPointPatrol()
    {
        _goingToB = startTowardB;
        _target = ResolveTarget();

        AlignFacingToTargetOnce();
    }

    private Transform ResolveTarget()
    {
        if (!useTwoPointPatrol) return null;
        if (!pointA || !pointB) return null;
        return _goingToB ? pointB : pointA;
    }

    /// <summary>
    /// 외부(상태머신/피격/대기 등)에서 이동을 멈추거나 재개.
    /// 멈춘 동안은 중력만 적용하고, 표면 반응/이동/목표 갱신을 중단합니다.
    /// </summary>
    public void SetStopped(bool stop)
    {
        _stopped = stop;

        if (stop)
        {
            Vector3 v = _rb.velocity;
            v -= Vector3.Project(v, transform.forward);
            _rb.velocity = v;
        }
        else
        {
            _target = ResolveTarget();
        }
    }

    /// <summary>
    /// 스턴 요청.
    /// on=true: 즉시 스턴 진입(벽에서 떨어지게), 중력은 월드 아래 고정, 바닥 접지 후 stunSeconds(최소 1초) 유지.
    /// on=false: 즉시 스턴 해제(강제).
    /// </summary>
    public void RequestStun(bool on, float stunSeconds)
    {
        if (!on)
        {
            _stunned = false;
            _stunGroundedOnce = false;
            return;
        }

        _stunned = true;

        // "바닥에 닿은 후" 유지시간은 최소 1초
        _stunHoldSeconds = Mathf.Max(1f, stunSeconds);

        // 스턴 중 상태 초기화
        _stunGroundedOnce = false;
        _tStunGrounded = 0f;

        // 벽에 붙는 동작을 즉시 끊기 위해: 회전/모드 커밋/대기 등을 정리
        isRotating = false;
        _hasPendingMode = false;
        _waiting = false;

        // 표면 모드는 일단 Walk로 되돌려 "클라임 유지"를 끊음
        _mode = Mode.Walk;
    }

    /// <summary>
    /// patrol 목표를 런타임에 지정하고 싶을 때 사용
    /// </summary>
    public void SetPatrolPoints(Transform a, Transform b, bool startToB = true)
    {
        pointA = a;
        pointB = b;
        useTwoPointPatrol = (a && b);
        startTowardB = startToB;

        _goingToB = startTowardB;
        _target = ResolveTarget();
        _waiting = false;
    }

    private void FixedUpdate()
    {
        if (lockZ)
        {
            Vector3 p = _rb.position;
            p.z = zValue;
            _rb.position = p;
        }

        // ===== 스턴이면: 무조건 월드 아래 중력 + upright 회전 + 접지 후 타이머 =====
        if (_stunned)
        {
            ApplyStunGravityWorldDown();
            StunUprightRotate();

            bool grounded = CheckGroundedWorldDown(out RaycastHit gHit);

            if (grounded)
            {
                if (!_stunGroundedOnce)
                {
                    _stunGroundedOnce = true;
                    _tStunGrounded = Time.time;
                }

                // 바닥 닿은 후 최소 1초(또는 더 긴 요청 시간) 유지
                if (Time.time - _tStunGrounded >= _stunHoldSeconds)
                {
                    _stunned = false;
                    // 스턴 해제 후에도 즉시 벽 붙기 난리를 막고 싶으면,
                    // 필요 시 여기서 1프레임 딜레이/쿨다운을 넣을 수 있습니다.
                }
            }

            return;
        }

        // ===== 일반 중력(현재 표면 기준) =====
        ApplyCustomGravity();

        // 회전 중이면 회전만 진행
        if (isRotating)
        {
            if (!stopMoveWhileRotating && !_stopped && !_waiting)
                MoveAlongForward(_lockedForward);
            return;
        }

        // 정지 중이면 "중력만"
        if (_stopped)
            return;

        // 대기 처리
        if (_waiting)
        {
            if (Time.time - _tWaitStart >= dwellTime)
            {
                _waiting = false;
                SwitchTarget();
            }
            else
            {
                return;
            }
        }

        // 목표 기반 dirSign
        UpdateDirSignByTarget();

        Vector3 forward = transform.forward;

        switch (_mode)
        {
            case Mode.Walk: WalkStep(forward); break;
            case Mode.Climb: ClimbStep(forward); break;
            case Mode.DownClimb: DownClimbStep(forward); break;
        }

        MoveAlongForward(forward);
        CheckArriveAndPingPong();
    }

    // ----------------------------
    // Stun helpers
    // ----------------------------
    private void ApplyStunGravityWorldDown()
    {
        _rb.AddForce(Vector3.down * gravityAccel, ForceMode.Acceleration);
    }

    private void StunUprightRotate()
    {
        // 월드 up에 맞게 upright로 복귀(2.5D면 yaw 고정이 필요할 수도 있으나, 여기서는 "up"만 맞춤)
        Quaternion target = Quaternion.FromToRotation(transform.up, Vector3.up) * _rb.rotation;

        float stepDeg = rotateDegPerSec * stunUprightRotateMul * Time.fixedDeltaTime;
        Quaternion next = Quaternion.RotateTowards(_rb.rotation, target, stepDeg);
        _rb.MoveRotation(next);
    }

    private bool CheckGroundedWorldDown(out RaycastHit hit)
    {
        // 월드 아래로 구체캐스트: 바닥 접지 판정
        Vector3 origin = transform.position; // 중심 기준
        Vector3 dir = Vector3.down;

        bool ok = Physics.SphereCast(
            origin,
            probeRadius * 0.9f,
            dir,
            out hit,
            stunGroundCheckDist,
            layer,
            QueryTriggerInteraction.Ignore
        );

        if (drawDebug)
        {
            Debug.DrawRay(origin, dir * stunGroundCheckDist, ok ? Color.green : Color.red);
            if (ok) Debug.DrawRay(hit.point, hit.normal * 0.2f, Color.cyan);
        }

        if (!ok) return false;

        // 월드 up 기준으로 "바닥"인지 확인
        float ang = Vector3.Angle(hit.normal, Vector3.up);
        return ang <= maxWalkSlopeDeg;
    }

    // ----------------------------
    // Patrol
    // ----------------------------
    private void UpdateDirSignByTarget()
    {
        if (!useTwoPointPatrol) return;
        if (!_target) return;

        Vector3 to = _target.position - transform.position;
        to.z = 0f;

        Vector3 fwd = transform.forward;
        fwd.z = 0f;

        if (fwd.sqrMagnitude < 1e-6f) return;

        float d = Vector3.Dot(to, fwd.normalized);

        const float dead = 0.01f;
        if (d > dead) _dirSign = 1;
        else if (d < -dead) _dirSign = -1;
    }

    private void CheckArriveAndPingPong()
    {
        if (!useTwoPointPatrol) return;
        if (!_target) return;

        Vector3 p = transform.position;
        Vector3 t = _target.position;
        p.z = 0f;
        t.z = 0f;

        float dist = Vector3.Distance(p, t);
        Debug.Log(dist);
        if (dist <= arriveEps)
        {
            if (dwellTime > 0f)
            {
                _waiting = true;
                _tWaitStart = Time.time;

                Vector3 v = _rb.velocity;
                v -= Vector3.Project(v, transform.forward);
                _rb.velocity = v;

                return;
            }

            SwitchTarget();
            return;
        }
    }

    private void SwitchTarget()
    {
        _goingToB = !_goingToB;
        _target = ResolveTarget();

        // 요청하신 동작: 목표 도착 시 즉시 y 180도 회전
        RotateYaw180Immediate();
    }

    // ----------------------------
    // Forces
    // ----------------------------
    private void ApplyCustomGravity()
    {
        _rb.AddForce(-transform.up * gravityAccel, ForceMode.Acceleration);
    }
    private void AlignFacingToTargetOnce()
    {
        if (!_target) return;

        // 2.5D에서 좌/우 판단은 x만으로 충분
        float dx = _target.position.x - transform.position.x;

        // 현재 forward가 목표의 x방향과 반대면 180도 즉시 회전
        // (forward가 +x이면 transform.forward.x > 0, -x이면 < 0)
        if (dx == 0f) return;

        bool targetOnRight = dx > 0f;
        bool facingRight = transform.forward.x > 0f;

        if (targetOnRight != facingRight)
            RotateYaw180Immediate();
    }

    private void RotateYaw180Immediate()
    {
        // transform.rotation.y 직접 수정 금지(Quaternion 성분임)
        Quaternion rot = _rb ? _rb.rotation : transform.rotation;
        Quaternion next = Quaternion.AngleAxis(180f, Vector3.up) * rot;

        if (_rb) _rb.MoveRotation(next);
        else transform.rotation = next;

        // 회전 중 상태가 꼬이지 않게 정리
        isRotating = false;
        _hasPendingMode = false;
    }

    private void MoveAlongForward(Vector3 forward)
    {
        Vector3 v = _rb.velocity;
        float curF = Vector3.Dot(v, forward);
        float add = (moveSpeed - curF);
        _rb.AddForce(forward * add * 20f, ForceMode.Acceleration);
    }

    // ----------------------------
    // Walk
    // ----------------------------
    private void WalkStep(Vector3 forward)
    {
        if (TryProbeSurface(ProbeOriginForward(), forward, surfaceSnapDistance, out RaycastHit wallHit))
        {
            if (IsWallLike(wallHit.normal))
            {
                RequestRotateToNormal(wallHit, Mode.Climb, forward);
                transform.position = wallHit.point;
                return;
            }
        }

        bool cliff = IsCliffAhead();
        if (cliff)
        {
            if (TryFindNextSurface(preferWall: true, out RaycastHit next))
            {
                RequestRotateToNormal(next, Mode.DownClimb, forward);
                return;
            }

            _dirSign *= -1;
        }

        if (TryProbeSurface(ProbeOriginFoot(), -transform.up, surfaceSnapDistance, out RaycastHit groundHit))
        {
            if (IsGroundLike(groundHit.normal))
                RequestRotateToNormal(groundHit, Mode.Walk, forward);
        }
    }

    // ----------------------------
    // Climb
    // ----------------------------
    private void ClimbStep(Vector3 forward)
    {
        if (TryProbeSurface(ProbeOriginFoot(), -transform.up, surfaceSnapDistance, out RaycastHit footHit))
        {
            if (IsGroundLike(footHit.normal))
            {
                RequestRotateToNormal(footHit, Mode.Walk, forward);
                return;
            }
        }

        if (TryProbeSurface(ProbeOriginForward(), forward, surfaceSnapDistance, out RaycastHit wallHit))
        {
            if (IsWallLike(wallHit.normal))
            {
                RequestRotateToNormal(wallHit, Mode.Climb, forward);
                return;
            }
        }

        if (TryFindNextSurface(preferWall: false, out RaycastHit next))
        {
            Mode nextMode = IsGroundLike(next.normal) ? Mode.Walk : Mode.Climb;
            RequestRotateToNormal(next, nextMode, forward);
            return;
        }

        _dirSign *= -1;
    }

    // ----------------------------
    // DownClimb
    // ----------------------------
    private void DownClimbStep(Vector3 forward)
    {
        if (TryProbeSurface(ProbeOriginFoot(), -transform.up, surfaceSnapDistance, out RaycastHit footHit))
        {
            if (IsGroundLike(footHit.normal))
            {
                RequestRotateToNormal(footHit, Mode.Walk, forward);
                return;
            }
        }

        if (TryProbeSurface(ProbeOriginForward(), forward, surfaceSnapDistance, out RaycastHit wallHit))
        {
            if (IsWallLike(wallHit.normal))
            {
                RequestRotateToNormal(wallHit, Mode.DownClimb, forward);
                return;
            }
        }

        if (TryFindNextSurface(preferWall: true, out RaycastHit next))
        {
            Mode nextMode = IsGroundLike(next.normal) ? Mode.Walk : Mode.DownClimb;
            RequestRotateToNormal(next, nextMode, forward);
            return;
        }

        _dirSign *= -1;
    }

    // ----------------------------
    // Probes
    // ----------------------------
    private Vector3 ProbeOriginFoot()
        => transform.position - transform.up * footProbeOffset;

    private Vector3 ProbeOriginForward()
        => ProbeOriginFoot() + transform.forward * forwardProbeOffset;

    private bool TryProbeSurface(Vector3 origin, Vector3 dir, float dist, out RaycastHit hit)
    {
        bool ok = Physics.Raycast(origin, dir, out hit, dist, layer, QueryTriggerInteraction.Ignore);

        if (drawDebug)
        {
            Debug.DrawRay(origin, dir.normalized * dist, ok ? Color.green : Color.red);
            if (ok) Debug.DrawRay(hit.point, hit.normal * 0.2f, Color.cyan);
        }
        return ok;
    }

    private bool IsGroundLike(Vector3 normal)
    {
        float ang = Vector3.Angle(normal, transform.up);
        return ang <= maxWalkSlopeDeg;
    }

    private bool IsWallLike(Vector3 normal)
    {
        float ang = Vector3.Angle(normal, transform.up);
        return ang >= wallMinSteepDeg;
    }

    private bool IsCliffAhead()
    {
        Vector3 origin = ProbeOriginFoot() + transform.forward * edgeForward;
        Vector3 dir = -transform.up;

        bool ok = Physics.SphereCast(origin, probeRadius * 0.9f, dir, out RaycastHit hit, edgeDown, layer, QueryTriggerInteraction.Ignore);

        if (drawDebug)
        {
            Debug.DrawRay(origin, dir * edgeDown, ok ? Color.yellow : Color.magenta);
            if (ok) Debug.DrawRay(hit.point, hit.normal * 0.2f, Color.blue);
        }

        return !ok || !IsGroundLike(hit.normal);
    }

    private bool TryFindNextSurface(bool preferWall, out RaycastHit best)
    {
        best = default;
        bool found = false;
        float bestScore = float.NegativeInfinity;

        Vector3 f = transform.forward;
        Vector3 d = -transform.up;

        Vector3[] dirs =
        {
            f,
            d,
            (f + d).normalized,
            (-f + d).normalized
        };

        Vector3 origin = ProbeOriginFoot();

        for (int i = 0; i < dirs.Length; i++)
        {
            if (!TryProbeSurface(origin, dirs[i], surfaceSnapDistance * 2f, out RaycastHit hit))
                continue;

            float wallness = Mathf.Abs(Vector3.Dot(hit.normal, transform.up));
            float score = preferWall ? (1f - wallness) : wallness;
            score -= hit.distance * 0.25f;

            if (!found || score > bestScore)
            {
                found = true;
                bestScore = score;
                best = hit;
            }
        }

        return found;
    }

    // ----------------------------
    // Rotation request & commit
    // ----------------------------
    private void RequestRotateToNormal(RaycastHit surfaceHit, Mode modeAfterRotate, Vector3 currentForward)
    {
        if (isRotating) return;

        Vector3 surfaceNormal = surfaceHit.normal;

        Quaternion target = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        float ang = Quaternion.Angle(_rb.rotation, target);

        if (ang <= angleThresholdDeg)
        {
            _rb.MoveRotation(target);
            _mode = modeAfterRotate;
            return;
        }

        _rotTarget = target;
        _pendingModeAfterRotate = modeAfterRotate;
        _hasPendingMode = true;

        _lockedForward = currentForward.normalized;

        StartCoroutine(RotateToTargetCoroutine());
    }

    private IEnumerator RotateToTargetCoroutine()
    {
        isRotating = true;

        while (Quaternion.Angle(_rb.rotation, _rotTarget) > angleThresholdDeg)
        {
            float stepDeg = rotateDegPerSec * Time.fixedDeltaTime;
            Quaternion next = Quaternion.RotateTowards(_rb.rotation, _rotTarget, stepDeg);
            _rb.MoveRotation(next);
            yield return new WaitForFixedUpdate();
        }

        _rb.MoveRotation(_rotTarget);

        if (_hasPendingMode)
        {
            _mode = _pendingModeAfterRotate;
            _hasPendingMode = false;
        }

        isRotating = false;
    }
}
