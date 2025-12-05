using System;
using UnityEngine;

public enum StateInfo { Idle, Attack, Move, Hit, Death }

[RequireComponent(typeof(RigidNavigation))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(EnemyHealth))]
[DisallowMultipleComponent]
public abstract class EnemyBase : MonoBehaviour, IExplosionInteract, IAttackable
{
    // ===== 공통 레퍼런스 =====
    protected RigidPlayerManagement _player;
    protected Animator _animator;
    protected RigidNavigation _nav;
    protected CapsuleCollider _capsule;
    protected Rigidbody _rigid;
    protected CameraShakeNoise _shakeSource;

    // 모듈
    protected EnemyHealth _health;
    protected EnemySight _sight;
    protected EnemyPatrol _patrol;
    protected EnemyChargeDeath _chargeDeath;

    // ===== 범위 =====
    [Header("Ranges")]
    [SerializeField] protected float _findRange = 1.5f;
    [SerializeField] protected float _aggroRange = 5f;
    [SerializeField] protected float _attackRange = 2.5f;
    protected Vector2 _lastHitDir2D = Vector2.zero;
    protected float _lastKnockbackForce = 0f;

    public Vector2 LastHitDir2D => _lastHitDir2D;
    public float LastKnockbackForce => _lastKnockbackForce;

    public float AggroRange => _aggroRange;
    public float AttackRange => _attackRange;

    // 외부 접근용 프로퍼티
    public float MaxHp => _health ? _health.MaxHp : 0f;
    public float Hp => _health ? _health.Hp : 0f;

    public Animator Anim => _animator;
    public RigidNavigation Nav => _nav;
    public RigidPlayerManagement Player => _player;
    public EnemySight Sight => _sight;
    public EnemyPatrol Patrol => _patrol;

    protected bool _isDead;

    // ===== FSM =====
    protected readonly EnemyStateMachine _fsm = new EnemyStateMachine();
    public StateInfo CurrentState => _fsm.Current?.Kind ?? StateInfo.Idle;

    public int RequiredAmpPts => throw new NotImplementedException();

    public int RequiredPerPts => throw new NotImplementedException();

    public int RequiredWavPts => throw new NotImplementedException();

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _nav = GetComponent<RigidNavigation>();
        _capsule = GetComponent<CapsuleCollider>();
        _rigid = GetComponent<Rigidbody>();

        _health = GetComponent<EnemyHealth>();
        _sight = GetComponent<EnemySight>();
        _patrol = GetComponent<EnemyPatrol>();
        _chargeDeath = GetComponent<EnemyChargeDeath>();

        _shakeSource = FindObjectOfType<CameraShakeNoise>();
    }

    protected virtual void OnEnable()
    {
        _isDead = false;

        if (GameManager.Instance)
            _player = GameManager.Instance.player;
        if(_player == null)
            _player = GameObject.Find("Player").GetComponent<RigidPlayerManagement>();
        if (_patrol != null)
            _patrol.InitPatrolPoints();

        if (_nav != null)
            _nav.SetEnabled(true);

        // Health 이벤트 연결
        if (_health != null)
        {
            _health.OnHpChangedEx += HandleHpChanged;
            _health.OnHit += HandleHit;
            _health.OnDied += HandleDied;
        }

        // 파생 클래스에서 상태 등록
        RegisterStates(_fsm);
        _fsm.Initialize(StateInfo.Idle);
    }

    protected virtual void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHpChangedEx -= HandleHpChanged;
            _health.OnHit -= HandleHit;
            _health.OnDied -= HandleDied;
        }
    }

    protected virtual void Update()
    {
        _fsm.Tick(Time.deltaTime);
    }

    /// <summary>파생 클래스에서 상태 객체를 등록</summary>
    protected abstract void RegisterStates(EnemyStateMachine fsm);

    protected void SetState(StateInfo next) => _fsm.Change(next);

    // ===== Health 이벤트 처리 =====
    private void HandleHpChanged(float hp, float maxHp, float norm)
    {
        
    }

    private void HandleHit(DamageInfo info)
    {
        if (_isDead) return;

        // 1) DamageInfo.SourceDir, KnockbackForce를 캐싱
        //    SourceDir은 "넉백 방향"이라고 하셨으니 그대로 신뢰합니다.
        Vector2 dir2D = new Vector2(info.SourceDir.x, info.SourceDir.y);

        if (dir2D.sqrMagnitude < 1e-4f)
        {
            // 거의 0벡터면 폴백: 적이 보고 있는 방향 기준
            float facing = Mathf.Sign(transform.localScale.x == 0 ? 1f : transform.localScale.x);
            dir2D = new Vector2(facing, 0f);
        }

        _lastHitDir2D = dir2D.normalized;
        _lastKnockbackForce = info.KnockbackForce;

        // 2) 상태 전환용 훅
        OnHit(info);
    }


    private void HandleDied(DamageInfo info)
    {
        if (_isDead) return;
        _isDead = true;

        SetState(StateInfo.Death);

        //_shakeSource?.ShakeOnChargeKill();

        if (info.IsCharge && _chargeDeath != null)
        {
            CinemachineEventReader.Instance.PlayBuiltInEvent(BuiltInEvents.ChargeKill);
            _chargeDeath.StartFlight(info.SourceDir, info.KnockbackForce);
        }

        OnDie(info);
    }

    /// <summary>피격 반응 (기본: Hit 상태 전환)</summary>
    protected virtual void OnHit(in DamageInfo info)
    {
        SetState(StateInfo.Hit);
    }

    /// <summary>사망 처리 (필요하면 파생에서 override)</summary>
    protected virtual void OnDie(in DamageInfo info)
    {
        // 예: gameObject.SetActive(false);
    }

    // IAttackable 프록시
    void IAttackable.TakeDamage(in DamageInfo info)
    {
        _health?.ApplyDamage(info);
    }

    public virtual void OnExplosionInteract(Channel channel)
    {
        // 필요 시 파생 클래스에서 구현
    }
}
