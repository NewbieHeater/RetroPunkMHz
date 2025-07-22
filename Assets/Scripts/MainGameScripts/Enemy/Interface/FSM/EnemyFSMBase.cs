using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public enum State { Patrol, Chase, Idle, MeleeAttack, RangeAttack, Search, Death, ANY }


[System.Serializable]
public struct PatrolPoint
{
    [Tooltip("순찰할 위치")]
    public Transform point;

    [Tooltip("이 위치에서 기다릴 시간(초)")]
    public float dwellTime;

    [Tooltip("이 위치로 이동할 때 점프가 필요한가?")]
    public bool needJump;

    [Tooltip("점프 높이 (needJump == true 일 때만)")]
    public float jumpPower;
}

public abstract class EnemyFSMBase : MonoBehaviour, IAttackable, IExplosionInteract
{
    [SerializeField] private EnemyIdleSOBase    enemyIdleBase;
    [SerializeField] private EnemyAttackSOBase  enemyAttackBase;
    [SerializeField] private EnemyPatrolSOBase  enemyPatrolBase;

    [HideInInspector] public EnemyIdleSOBase enemyIdleBaseInstance;
    [HideInInspector] public EnemyAttackSOBase enemyAttackBaseInstance;
    [HideInInspector] public EnemyPatrolSOBase enemyPatrolBaseInstance;

    public bool stop;
    [Header("스탯 설정")]
    [SerializeField] protected TextMeshProUGUI HpBar;
    [SerializeField] protected int          maxHp = 100;
    [SerializeField] protected int          currentHp;
    public bool                          isDead;

    [Header("점프 설정")]
    [Tooltip("포물선 최고점까지 높이")]
    [SerializeField] public float        defaultApexHeight = 2f;
    [Tooltip("왕복여부(False시 순환)")]
    [SerializeField] public bool         getBackAvailable = false;

    [Header("넉백/폭발 설정")]
    [SerializeField] protected bool         explodeOnWall;
    [SerializeField] protected float        collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float        explosionRadius = 3f;

    [Header("시야 설정")]
    [Tooltip("에너미가 플레이어를 볼 수 있는 최대 각도(도)")]
    [SerializeField] protected float        viewAngle = 45f;
    [SerializeField] protected LayerMask    obstacleMask;
    

    [Header("Ranges")]
    [SerializeField] protected float findRange = 1.5f;
    [SerializeField] protected float meleeAttackRange = 2f;
    [SerializeField] protected float rangeAttackRange = 2f;
    [SerializeField] protected float aggroRange = 5f;
    [SerializeField] protected float maxDist = 0;

    public PatrolPoint[] patrolPoints;
    public CapsuleCollider  cap;
    public Rigidbody        rigid { get; protected set; }
    public NavMeshAgent     agent { get; protected set; }
    public Animator         anime { get; protected set; }
    public RigidNavigation  rigidNav;

    public virtual int RequiredAmpPts => 0;
    public virtual int RequiredPerPts => 0;
    public virtual int RequiredWavPts => 0;

    public State CurrentState { get; protected set; }

    protected DataStateMachine fsm;
    protected Dictionary<State, BaseState> stateMap;
    protected List<StateTransition> transitions;

    public PlayerManagement player;


    protected virtual void Awake()
    {
        enemyIdleBaseInstance = Instantiate(enemyIdleBase);
        enemyAttackBaseInstance = Instantiate(enemyAttackBase);
        enemyPatrolBaseInstance = Instantiate(enemyPatrolBase);

        maxDist = aggroRange + 1f;

        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();
        cap   = GetComponent<CapsuleCollider>();
        rigidNav = GetComponent<RigidNavigation>();

        currentHp = maxHp;
        UpdateHpBarText();
        
    }

    protected virtual void Start()
    {
        enemyIdleBaseInstance.Initialize(gameObject, this);
        enemyAttackBaseInstance.Initialize(gameObject, this);
        enemyPatrolBaseInstance.Initialize(gameObject, this);
    }

    protected virtual void OnEnable()
    {
    }
    protected bool IsPlayerInSight(float range)
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > range)
            return false;

        Vector3 toPlayer = (player.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        Debug.DrawRay(transform.position + Vector3.up, toPlayer * dist, Color.green, 0.1f);
        if (angle > viewAngle)
        {
            Debug.DrawRay(transform.position + Vector3.up, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        Vector3 origin = transform.position;
        Vector3 target = player.transform.position + Vector3.up;
        Vector3 dir = target - origin;
        Debug.DrawRay(origin, dir.normalized * range, Color.red, 0.1f);

        return IsRayHitOnPlayer(origin, dir);
    }

    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir)
    {
        Debug.DrawRay(origin, dir.normalized * maxDist, Color.red, 0.1f);

        if (Physics.Raycast(origin, dir, out var hit, maxDist, obstacleMask))
        {
            return hit;
        }

        return null;
    }

    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir)
    {
        if (GetRaycastHit(origin, dir) is RaycastHit hit)
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }


    #region StateUpdate

    private void Update()
    {
        fsm.UpdateState();
    }

    private void FixedUpdate()
    {
        fsm.FixedUpdateState();
    }

    public void ChangeState(State next)
    {
        fsm.ChangeState(next);
        CurrentState = next;
    }
    #endregion

    public virtual void OnExplosionInteract(Channel channel)
    {
    }

    #region knockback
    public void ApplyKnockback(Vector3 direction, float force)
    {
        cap.enabled = true;
        rigid.velocity = Vector3.zero;
        rigid.isKinematic = false;
        rigid.useGravity = true;
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigid.AddForce(direction.normalized * force / 1.3f , ForceMode.Impulse);

        explodeOnWall = true;
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
        var hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IExplosionInteract>(out var reactable))
                reactable.OnExplosionInteract(ChannelManager.Instance.CurrentChannel);
        }
        Destroy(gameObject);
    }
    #endregion

    #region 데미지처리
    public void TakeDamage(in DamageInfo info)
    {
        if (isDead) return;

        currentHp -= info.Amount;
        Debug.Log("hit");
        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;

            if (info.IsCharge)
            {
                ApplyKnockback(info.SourceDir.normalized, info.KnockbackForce);
            }
            else
            {
                DieInstant();
            }
        }

        UpdateHpBarText();
    }

    private void UpdateHpBarText()
    {
        HpBar.text = $"Hp : {currentHp}";
    }

    private void DieInstant()
    {
        Destroy(gameObject);
    }
    #endregion

    public virtual void SetQuestionMark(bool active)
    {
        
    }
}
