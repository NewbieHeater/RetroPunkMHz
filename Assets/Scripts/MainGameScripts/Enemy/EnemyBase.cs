using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum StateInfo { Idle, Attack, Move, Hit, Death }
public class EnemyRuntimeState
{
    public Vector3 position;
    public float hp;
    public bool isDead;
}

[RequireComponent(typeof(RigidNavigation))]
[RequireComponent(typeof(CapsuleCollider))]
[DisallowMultipleComponent]
public abstract class EnemyBase : MonoBehaviour, IAttackable, IExplosionInteract
{
    // ====== 공통 레퍼런스 ======
    protected RigidPlayerManagement _player;
    protected Animator _animator;
    protected RigidNavigation _nav;
    protected CapsuleCollider _capsule;
    protected Rigidbody _rigid; // 있으면 사용(권장)

    // ====== 공통 설정 ======
    [Header("Ranges")]
    [SerializeField] protected float _findRange = 1.5f;
    [SerializeField] protected float _aggroRange = 5f;
    [SerializeField] protected float _attackRange = 2.5f;

    [Header("Sight")]
    [SerializeField] protected float _viewAngle = 45f;
    [SerializeField] protected LayerMask _obstacleMask;

    [Header("HP/Explosion")]
    [SerializeField] protected float _maxHp = 100f;
    [SerializeField] protected float _explosionRadius = 0f; // 0이면 미사용

    [Header("Charged-Death Explosion")]
    [SerializeField] LayerMask _explodeOnHitMask; // 어떤 레이어와 부딪히면 터질지
    [SerializeField] float _chargedDeathTimeout = 4.0f; // 충돌 못하면 이 시간 뒤 정리

    void IAttackable.TakeDamage(in DamageInfo info) => TakeDamage(info);
    bool _chargedDeathArmed;   // 차징 즉사 ‘비행’ 상태

    public float MaxHp
    {
        get => _maxHp;
        set
        {
            value = Mathf.Max(1f, value);
            if (Mathf.Approximately(value, _maxHp)) return;
            _maxHp = value;
            // Max 변경 시 현재 hp도 재클램프 + 이벤트 재발행
            Hp = Mathf.Min(_hp, _maxHp);
        }
    }

    protected float _hp = 100;
    public float Hp
    {
        get => _hp;
        protected set
        {
            float clamped = Mathf.Clamp(value, 0f, _maxHp);
            if (Mathf.Approximately(clamped, _hp)) return; // 동일 값이면 무의미한 이벤트 방지
            _hp = clamped;

            float norm = (_maxHp > 0f) ? (_hp / _maxHp) : 0f;
            OnHpChangedEx?.Invoke(_hp, _maxHp, norm);
        }
    }


    protected bool _isDead;

    // ====== FSM ======
    protected StateInfo _state;
    protected float _stateTime; // 상태 경과시간

    // ====== 순찰(필요 시 사용) ======
    [Header("Patrol")]
    [SerializeField] protected PathProvider pathProvider;
    [SerializeField] protected float moveSpeed = 0.8f;
    [SerializeField] protected float rotateDegPerSec = 180f;
    [SerializeField] protected bool pingPong = true;
    [SerializeField] protected float arriveEps = 0.05f;
    [SerializeField] protected float minStartDist = 0.08f; // 전환시 최소 이동
    [SerializeField] protected float minProgressAfterAdvance = 0.08f;

    protected IReadOnlyList<Vector3> pts = System.Array.Empty<Vector3>();
    protected PatrolPoint[] defs = System.Array.Empty<PatrolPoint>();
    protected int idx; protected bool forward = true;
    bool _waiting; float _tWait;
    Vector3 _lastAdvancePos; bool _progressGateArmed;

    public int RequiredAmpPts => throw new System.NotImplementedException();

    public int RequiredPerPts => throw new System.NotImplementedException();

    public int RequiredWavPts => throw new System.NotImplementedException();

    public event Action<float, float, float> OnHpChangedEx;

    // ====== Unity ======
    protected virtual void Awake()
    {
        _hp = _maxHp;
        _animator = GetComponentInChildren<Animator>();
        _nav = GetComponent<RigidNavigation>();
        _capsule = GetComponent<CapsuleCollider>();
        _rigid = GetComponent<Rigidbody>();
        _shakeSource = FindObjectOfType<CameraShakeNoise>();
    }
    
    protected virtual void OnEnable()
    {
        _hp = _maxHp;
        _isDead = false;

        InitPatrolPoints();
        if (_nav) _nav.SetEnabled(true);
        // 외부 참조는 여기나 Start에서
        if (GameManager.Instance)
            _player = GameManager.Instance.player;

        // 초기 상태 브로드캐스트 (UI가 늦게 붙어도 이후 OnEnable에서 다시 받음)
        OnHpChangedEx?.Invoke(_hp, _maxHp, (_maxHp > 0f) ? (_hp / _maxHp) : 0f);
    }



    protected virtual void Start()
    {
        // 씬 전역이 준비된 뒤 필요한 초기화
        if (!_player && GameManager.Instance) _player = GameManager.Instance.player;
    }

    
    protected virtual void Update()
    {
        _stateTime += Time.deltaTime;
        OnTick(); // 템플릿 메서드 → 각 상태 Tick 분배
    }

    // ====== FSM 템플릿 ======
    protected void SetState(StateInfo next)
    {
        if (_state == next) return;
        OnExitState(_state);
        _state = next;
        _stateTime = 0f;
        OnEnterState(_state);
    }

    // 공통 진입/종료 훅 (자식에서 base 호출 가능)
    protected virtual void OnEnterState(StateInfo s) { }
    protected virtual void OnExitState(StateInfo s) { }

    // 상태별 Tick 분배 템플릿
    void OnTick()
    {
        switch (_state)
        {
            case StateInfo.Idle: TickIdle(); break;
            case StateInfo.Move: TickMove(); break;
            case StateInfo.Attack: TickAttack(); break;
            case StateInfo.Hit: TickHit(); break;
            case StateInfo.Death: TickDeath(); break;
        }
    }

    // ====== 상태별 가상 Tick (자식이 필요만 오버라이드) ======
    protected virtual void TickIdle()
    {
        // 기본 아이들: 플레이어 감지되면 공격, 아니면 이동
        if (IsPlayerInSight(_attackRange))
            SetState(StateInfo.Attack);
        else if (!IsPlayerInSight(_aggroRange))
            SetState(StateInfo.Move);
    }
    protected virtual void TickMove() { PatrolTick(RigidNavigation.MoveMode.Walk); }
    protected virtual void TickAttack() { /* 자식에서 구현 */ }
    protected virtual void TickHit() { /* 자식에서 구현(넉백/히트스턴) */ }
    protected virtual void TickDeath() { /* 기본: 아무것도 안 함 */ }

    // ====== 시야/LoS ======
    protected bool IsPlayerInSight(float range)
    {
        if (!_player) return false;

        Vector3 eyePos = transform.position + Vector3.up;
        Vector3 playerEye = _player.transform.position + Vector3.up;
        Vector3 toPlayer = playerEye - eyePos;

        if (toPlayer.magnitude > range) return false;

        Vector3 fwdFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 toFlat = Vector3.ProjectOnPlane(toPlayer, Vector3.up).normalized;
        if (Vector3.Angle(fwdFlat, toFlat) > _viewAngle * 0.5f) return false;

        return HasLineOfSight(eyePos, toPlayer, toPlayer.magnitude);
    }

    bool HasLineOfSight(Vector3 origin, Vector3 dir, float dist)
    {
        int losMask = _obstacleMask | (1 << LayerMask.NameToLayer("Player"));
        if (Physics.Raycast(origin, dir.normalized, out var hit, dist, losMask, QueryTriggerInteraction.Ignore))
            return hit.collider.CompareTag("Player");
        return true; // 막힌 게 없음
    }

    // ====== 피해/사망/폭발/넉백 ======
    public void Heal(float amount) => Hp = Hp + Mathf.Max(0f, amount);
    public void Damage(float amount) => Hp = Hp - Mathf.Max(0f, amount);
    CameraShakeNoise _shakeSource;
    public virtual void TakeDamage(in DamageInfo info)
    {
        if (_isDead) return;
        Damage(info.Amount);

        if (Hp <= 0f)
        {
            SetState(StateInfo.Death);
            _isDead = true;

            _shakeSource.ShakeOnChargeKill();

            if (info.IsCharge)
                StartChargedDeathFlight(info.SourceDir, info.KnockbackForce);
            OnDie();
            return;
        }

        OnHit(info); // 히트 반응(자식에서 오버라이드 가능)
    }

    protected virtual void OnHit(in DamageInfo info)
    {
        SetState(StateInfo.Hit);
    }

    protected virtual void OnDie()
    {
        SetState(StateInfo.Death);
        //gameObject.SetActive(false);
    }

    public void StartChargedDeathFlight(Vector3 dir, float force)
    {
        _chargedDeathArmed = true;
        if (_nav) _nav.SetEnabled(false);

        Vector3 f = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.right;
        if (_rigid)
        {
            _rigid.isKinematic = false;
            _rigid.useGravity = true;
            _rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigid.velocity = new Vector3(0f, _rigid.velocity.y, 0f);
            _rigid.AddForce(dir.normalized * force, ForceMode.Impulse);
        }
        else
        {
            // 폴백: Transform 이동
            transform.position += dir.normalized * force * Time.deltaTime;
        }
        StartCoroutine(ChargedDeathTimeoutRoutine());
    }

    public virtual void OnExplosionInteract(Channel channel) { /* 필요 시 구현 */ }

    protected void Explode()
    {
        var hits = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (var h in hits)
            if (h.TryGetComponent<IExplosionInteract>(out var r))
                r.OnExplosionInteract(ChannelManager.Instance.CurrentChannel);
        Destroy(gameObject);
    }

    IEnumerator ChargedDeathTimeoutRoutine()
    {
        float t0 = Time.time;
        while (_chargedDeathArmed && Time.time - t0 < _chargedDeathTimeout)
            yield return null;

        _chargedDeathArmed = false;
        // 타임아웃: 그냥 제거(혹은 OnDie()로 전환하고 비활성화)
        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision c)
    {
        if (!_chargedDeathArmed) return;

        // 레이어 필터
        int otherLayer = c.gameObject.layer;
        if (((1 << otherLayer) & _explodeOnHitMask) == 0)
            return;

        // 바닥 무시 옵션
        if (c.collider.CompareTag("Ground"))
            return;

        _chargedDeathArmed = false;

        // 폭발 이펙트/데미지/상호작용
        if (_explosionRadius > 0f) Explode();
        else gameObject.SetActive(false);
    }


    // ====== 순찰 공통 ======
    protected void InitPatrolPoints()
    {
        if (!pathProvider)
        {
            pathProvider = GetComponent<PathProvider>();
            if (!pathProvider) { pts = System.Array.Empty<Vector3>(); defs = System.Array.Empty<PatrolPoint>(); return; }
        }
        defs = pathProvider.Definitions;
        pts = pathProvider.BuildWorldPoints(transform);

        idx = 0; forward = true;
        _waiting = false;
    }

    protected void BeginPatrol(RigidNavigation.MoveMode mode)
    {
        if (pts == null || pts.Count == 0 || !_nav) return;

        int startIdx = idx;

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

        _lastAdvancePos = transform.position;
        _progressGateArmed = true;
    }

    protected void PatrolTick(RigidNavigation.MoveMode mode)
    {
        if (pts == null || pts.Count == 0 || !_nav) return;

        // 대기
        if (_waiting)
        {
            if (Time.time - _tWait >= defs[idx].dwellTime)
            {
                _waiting = false;
                AdvanceOnce();
                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx], mode);
                _lastAdvancePos = transform.position;
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

            // 점프 필요 시 우선 처리
            if (defs[idx].needJump && forward)
            {
                AdvanceOnce();
                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx], RigidNavigation.MoveMode.Jump);
                _lastAdvancePos = transform.position;
                _progressGateArmed = true;
                return;
            }

            // 대기
            if (defs[idx].dwellTime > 0f)
            {
                _waiting = true;
                _tWait = Time.time;
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

    protected void FaceTowardsX(float tx)
    {
        float yaw = (tx - transform.position.x) >= 0f ? 90f : 270f;
        var target = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateDegPerSec * Time.deltaTime);
    }

    protected bool HasArrived()
    {
        if (_progressGateArmed)
        {
            if (Vector3.Distance(transform.position, _lastAdvancePos) < minProgressAfterAdvance)
                return false;
            _progressGateArmed = false;
        }

        float rem = _nav.RemainingDistanceVector3();
        if (float.IsNaN(rem) || float.IsInfinity(rem))
            rem = Mathf.Abs(pts[idx].x - transform.position.x);

        return rem <= arriveEps;
    }

    protected int FindNextUsableIndex(int startIdx)
    {
        if (pts == null || pts.Count == 0) return startIdx;
        int len = pts.Count;
        int i = startIdx; int guard = len;

        while (guard-- > 0)
        {
            float dx = Mathf.Abs(pts[i].x - transform.position.x);
            float dy = Mathf.Abs(pts[i].y - transform.position.y);
            if (dx < minStartDist && dy < minStartDist)
            {
                var nxt = GetNextIndexPingPong(i, forward, len);
                i = nxt.nextIdx; forward = nxt.nextForward;
                continue;
            }
            break;
        }
        return i;
    }

    (int nextIdx, bool nextForward) GetNextIndexPingPong(int i, bool fwd, int len)
    {
        if (!pingPong) return ((i + 1) % len, fwd);

        if (fwd)
        {
            if (i + 1 < len) return (i + 1, true);
            return (Mathf.Max(len - 2, 0), false);
        }
        else
        {
            if (i - 1 >= 0) return (i - 1, false);
            return (Mathf.Min(1, len - 1), true);
        }
    }

    protected void AdvanceOnce()
    {
        int len = (pts != null) ? pts.Count : 0;
        if (len <= 1) return;

        var nxt = GetNextIndexPingPong(idx, forward, len);
        idx = nxt.nextIdx;
        forward = nxt.nextForward;
    }

}
