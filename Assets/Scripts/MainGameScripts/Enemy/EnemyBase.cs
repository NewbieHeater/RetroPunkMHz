using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.WSA;
using static RigidNavigation;

public abstract class EnemyBase : MonoBehaviour, IAttackable, IExplosionInteract
{
    public int RequiredAmpPts => throw new System.NotImplementedException();
    public int RequiredPerPts => throw new System.NotImplementedException();
    public int RequiredWavPts => throw new System.NotImplementedException();

    protected RigidPlayerManagement _player;
    protected Animator _animator;
    protected RigidNavigation _nav;
    protected AnimatorProxy _animatorProxy;

    [Header("점프 설정")]
    [Tooltip("포물선 최고점까지 높이")]
    [SerializeField] public float _defaultApexHeight = 2f;
    [Tooltip("왕복여부(False시 순환)")]
    [SerializeField] public bool _getBackAvailable = false;

    [Header("Ranges")]
    [SerializeField] protected float _findRange = 1.5f;
    [SerializeField] protected float _meleeAttackRange = 2f;
    [SerializeField] protected float _rangeAttackRange = 2f;
    [SerializeField] protected float _aggroRange = 5f;
    [SerializeField] protected float _attackRange = 5f;
    [SerializeField] protected float _maxDist = 5;

    [Header("시야 설정")]
    [Tooltip("에너미가 플레이어를 볼 수 있는 최대 각도(도)")]
    [SerializeField] protected float _viewAngle = 45f;
    [SerializeField] protected LayerMask _obstacleMask;

    protected StateInfo _state;

    public void OnExplosionInteract(Channel channel)
    {
        throw new System.NotImplementedException();
    }

    protected virtual void Start()
    {
        _player = GameManager.Instance.player;
        _animator = GetComponentInChildren<Animator>();
        _capsule = GetComponent<CapsuleCollider>();
        _rigid = GetComponent<Rigidbody>();
        _nav = GetComponent<RigidNavigation>();
        _animatorProxy = GetComponent<AnimatorProxy>();

        InitPatrolPoints();           // ★ 추가: 순찰 포인트 초기화
        BeginPatrol(MoveMode.Walk);   // ★ 추가: 첫 목적지로 바로 출발
    }

    protected void Update()
    {
        FSM();
    }

    protected abstract void SetState(StateInfo next);
    protected abstract void EnterState(StateInfo next);
    protected abstract void ExitState(StateInfo next);
    protected abstract void FSM();

    // ===== LoS / 시야 =====
    protected bool HasLineOfSightToPlayer(float maxRange)
    {
        if (!_player) return false;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 target = _player.transform.position + Vector3.up;
        Vector3 dir = (target - origin);
        float dist = dir.magnitude;
        if (dist > maxRange) return false;

        int losMask = _obstacleMask | (1 << LayerMask.NameToLayer("Player"));
        if (Physics.Raycast(origin, dir.normalized, out var hit, dist, losMask, QueryTriggerInteraction.Ignore))
            return hit.collider.CompareTag("Player");

        return true; // 아무것도 안 맞으면 막힌 게 없음
    }

    protected bool IsPlayerInSight(float range)
    {
        if (!_player) return false;

        Vector3 eyePos = transform.position + Vector3.up;
        Vector3 playerEye = _player.transform.position + Vector3.up;
        Vector3 toPlayer = playerEye - eyePos;

        float dist = toPlayer.magnitude;
        if (dist > range) return false;

        Vector3 forwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 toPlayerFlat = Vector3.ProjectOnPlane(toPlayer, Vector3.up).normalized;
        float angle = Vector3.Angle(forwardFlat, toPlayerFlat);
        if (angle > _viewAngle * 0.5f) return false;

        return HasLineOfSightToPlayer(range);
    }

    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir, float distance)
    {
        Debug.DrawRay(origin, dir.normalized * distance, Color.red, 0.1f);
        if (Physics.Raycast(origin, dir, out var hit, distance, _obstacleMask))
            return hit;
        return null;
    }

    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir)
    {
        if (GetRaycastHit(origin, dir, _maxDist) is RaycastHit hit)
            return hit.collider.CompareTag("Player");
        return false;
    }

    CapsuleCollider _capsule;
    Rigidbody _rigid;
    bool isDead;
    float _explosionRadius;

    #region knockback
    public void ApplyKnockback(Vector3 direction, float force)
    {
        _capsule.enabled = true;
        _rigid.velocity = Vector3.zero;
        _rigid.isKinematic = false;
        _rigid.useGravity = true;
        _rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigid.AddForce(direction.normalized * force / 1.3f, ForceMode.Impulse);
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Ground") && isDead)
        {
            Explode();
        }
    }

    public void Explode()
    {
        var hits = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IExplosionInteract>(out var reactable))
                reactable.OnExplosionInteract(ChannelManager.Instance.CurrentChannel);
        }
        Destroy(gameObject);
    }

    private float mCurrentHp = 100;
    public virtual void TakeDamage(in DamageInfo info)
    {
        if (isDead) return;
        mCurrentHp -= info.Amount;
        Debug.Log("hit");

        if (mCurrentHp <= 0)
        {
            mCurrentHp = 0;
            isDead = true;

            if (info.IsCharge)
                ApplyKnockback(info.SourceDir.normalized, info.KnockbackForce);
            else
                DieInstant();
        }
    }

    private void DieInstant()
    {
        isDead = true;
        gameObject.SetActive(false);
    }

    void IAttackable.TakeDamage(in DamageInfo info) => TakeDamage(info);
    #endregion

    // ===== 순찰/이동 =====
    protected IReadOnlyList<Vector3> pts;
    PatrolPoint[] defs;
    [SerializeField] PathProvider pathProvider;

    [Header("Move/Rotate")]
    [SerializeField] float moveSpeed = 0.8f;
    [SerializeField] float rotateDegPerSec = 180f;
    [SerializeField] bool pingPong = true;

    protected int idx;
    bool forward = true;
    bool isWaiting = false;
    float tWait;

    [Header("Arrival")]
    [SerializeField] float arriveEps = 0.05f;

    Vector3 _lastAdvancePos;
    bool _progressGateArmed = false;
    [SerializeField] float _minProgressAfterAdvance = 0.08f;

    void InitPatrolPoints()
    {
        if (!pathProvider) pathProvider = GetComponent<PathProvider>();
        if (!pathProvider)
        {
            Debug.LogError("[EnemyBase] PathProvider가 없습니다.");
            pts = System.Array.Empty<Vector3>();
            defs = System.Array.Empty<PatrolPoint>();
            return;
        }
        defs = pathProvider.Definitions;
        pts = pathProvider.BuildWorldPoints(transform);

        idx = 0;
        forward = true;
        isWaiting = false;
    }

    protected void BeginPatrol(MoveMode mode)
    {
        if (pts == null || pts.Count == 0 || _nav == null) return;

        int startIdx = idx;

        // 암묵적 0(현재 위치)을 쓰는 경우: 바로 1로 시작
        if (pts.Count >= 2 &&
            Mathf.Abs(pts[0].x - transform.position.x) < arriveEps &&
            Mathf.Abs(pts[0].y - transform.position.y) < arriveEps)
        {
            startIdx = 1;
        }

        idx = FindNextUsableIndex(startIdx);

        _nav.isStopped = false;
        _nav.SetSpeed(moveSpeed);
        _nav.SetDestination(pts[idx], mode);

        _lastAdvancePos = transform.position; // 이중 도착 방지 게이트 장전
        _progressGateArmed = true;
    }


    // === 인덱스 전진(한 프레임 1회 보장) ===
    (int nextIdx, bool nextForward) GetNextIndexPingPong(int i, bool fwd, int len)
    {
        if (!pingPong) return ((i + 1) % len, fwd);

        if (fwd)
        {
            if (i + 1 < len) return (i + 1, true);
            // 끝점(len-1) 도착 → 한 칸 안쪽으로
            return (Mathf.Max(len - 2, 0), false);
        }
        else
        {
            if (i - 1 >= 0) return (i - 1, false);
            // 0 도착 → 한 칸 안쪽으로
            return (Mathf.Min(1, len - 1), true);
        }
    }


    void AdvanceOnce()
    {
        int len = pts.Count; if (len <= 1) return;
        var nxt = GetNextIndexPingPong(idx, forward, len);
        idx = nxt.nextIdx;
        Debug.Log(idx);
        forward = nxt.nextForward;
    }

    bool HasArrived()
    {
        if (_progressGateArmed)
        {
            if (Vector3.Distance(transform.position, _lastAdvancePos) < _minProgressAfterAdvance)
                return false; // 아직 거의 안 움직였음 → 도착 아님
            _progressGateArmed = false;
        }

        float rem = _nav.RemainingDistanceVector3();
        if (float.IsNaN(rem) || float.IsInfinity(rem))
            rem = Mathf.Abs(pts[idx].x - transform.position.x);

        return rem <= arriveEps;
    }

    void FaceTowardsX(float tx)
    {
        float yaw = (tx - transform.position.x) >= 0f ? 90f : 270f;
        var target = Quaternion.Euler(0, yaw, 0);
        RotateTowards(target, rotateDegPerSec * Time.deltaTime);
    }

    public void RotateTowards(Quaternion target, float maxDeg)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxDeg);
    }

    [SerializeField] float _minStartDistance = 0.08f; // 시작/전환시 최소 요구 이동

    int FindNextUsableIndex(int startIdx)
    {
        if (pts == null || pts.Count == 0) return startIdx;
        int len = pts.Count;
        int i = startIdx;
        int guard = len; // 무한루프 방지

        while (guard-- > 0)
        {
            float dx = Mathf.Abs(pts[i].x - transform.position.x);
            float dy = Mathf.Abs(pts[i].y - transform.position.y);

            // 현재 위치와 너무 가까운 목적지는 스킵
            if (dx < _minStartDistance && dy < _minStartDistance)
            {
                var nxt = GetNextIndexPingPong(i, forward, len);
                i = nxt.nextIdx;
                forward = nxt.nextForward;
                continue;
            }
            break;
        }
        return i;
    }


    public void Tick(MoveMode mode)
    {
        if (pts == null || pts.Count == 0 || _nav == null) return;

        // 대기
        if (isWaiting)
        {
            if (Time.time - tWait >= defs[idx].dwellTime)
            {
                isWaiting = false;
                AdvanceOnce();

                _nav.SetSpeed(moveSpeed);                // ★ 속도 보장
                _nav.SetDestination(pts[idx], mode);
                _lastAdvancePos = transform.position;    // ★ 게이트 장전
                _progressGateArmed = true;
            }
            else
            {
                FaceTowardsX(pts[idx].x);
            }
            return;
        }

        // 도착
        if (HasArrived())
        {
            _nav.ResetPath();

            // 점프 우선
            if (defs[idx].needJump && forward)
            {
                AdvanceOnce();
                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx], MoveMode.Jump);
                _lastAdvancePos = transform.position;
                _progressGateArmed = true;
                return;
            }

            // 대기
            if (defs[idx].dwellTime > 0f)
            {
                isWaiting = true;
                tWait = Time.time;
                return;
            }

            // 다음 포인트 즉시 전환
            AdvanceOnce();
            _nav.SetSpeed(moveSpeed);
            _nav.SetDestination(pts[idx], mode);
            _lastAdvancePos = transform.position;
            _progressGateArmed = true;
            return;
        }

        // 이동 중 회전
        FaceTowardsX(pts[idx].x);
    }
}
