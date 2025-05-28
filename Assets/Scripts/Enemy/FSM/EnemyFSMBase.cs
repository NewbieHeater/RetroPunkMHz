using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using static UnityEngine.UI.Image;

public enum State { Patrol, Chase, Idle, MeleeAttack, RangeAttack, Death, ANY }


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

public abstract class EnemyFSMBase : MonoBehaviour//, IExplosionInteract, IKnockbackable, IAttackable
{
    [Header("순찰 전략 (ScriptableObject)")]
    [Tooltip("에디터에서 PatrolBehaviorSO 할당")]
    public PatrolBehaviorSO patrolBehavior;
    public IdleBehaviorSO idleBehavior;
    public AttackBehaviorSO meleeAttackBehavior;
    public AttackBehaviorSO rangeAttackBehavior;
    [Header("스탯 설정")]
    [SerializeField] protected TextMeshProUGUI HpBar;
    [SerializeField] protected int maxHp = 100;
    protected int currentHp;
    protected bool isDead;

    [Header("점프 설정")]
    [Tooltip("포물선 최고점까지 높이 (웨이포인트별 jumpPower를 apex로 사용)")]
    public float defaultApexHeight = 2f;
    [Tooltip("왕복여부(False시 순환)")]
    public bool getBackAvailable = false;

    [Header("넉백/폭발 설정")]
    [SerializeField] protected bool explodeOnWall;
    [SerializeField] protected float collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float explosionRadius = 3f;

    [Header("시야 설정")]
    [Tooltip("에너미가 플레이어를 볼 수 있는 최대 각도(도)")]
    public float viewAngle = 45f;

    public GameObject questionMark;

    // 컴포넌트 참조
    public CapsuleCollider cap;
    public Rigidbody rigid { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public Animator anime { get; private set; }

    public virtual int RequiredAmpPts => 0;
    public virtual int RequiredPerPts => 0;
    public virtual int RequiredWavPts => 0;

    public abstract void Patrol();
    public abstract void Attack();

    public State CurrentState { get; private set; }

    private Dictionary<State, IState<EnemyFSMBase>> stateMap;
    private List<StateTransition<EnemyFSMBase>> transitions;
    private DataStateMachine<EnemyFSMBase> sm;
    private Dictionary<State, List<StateTransition<EnemyFSMBase>>> transitionsByState;

    public Player player;
    [Header("Ranges")]
    public float findRange = 1.5f;
    public float meleeAttackRange = 2f;
    public float rangeAttackRange = 2f;
    public float aggroRange = 5f;
    float maxDist = 0;
    public PatrolPoint[] patrolPoints;

    protected virtual void Awake()
    {
        maxDist = aggroRange + 1f;

        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();
        cap = GetComponent<CapsuleCollider>();

        patrolBehavior = Instantiate(patrolBehavior);
        patrolBehavior.Initialize(gameObject, this);
        idleBehavior = Instantiate(idleBehavior);
        idleBehavior.Initialize(gameObject, this);
        meleeAttackBehavior = Instantiate(meleeAttackBehavior);
        meleeAttackBehavior.Initialize(gameObject, this);
        rangeAttackBehavior = Instantiate(rangeAttackBehavior);
        rangeAttackBehavior.Initialize(gameObject, this);

        //agent.updateRotation = false;
        currentHp = maxHp;
        UpdateHpBarText();

        // 상태 인스턴스 매핑
        stateMap = new Dictionary<State, IState<EnemyFSMBase>>()
        {
            { State.Patrol,         new EnemyRobotState.PatrolState()       },
            { State.Chase,          new EnemyRobotState.MoveState()         },
            { State.Idle,           new EnemyRobotState.IdleState()         },
            { State.MeleeAttack,    new EnemyRobotState.AttackState()       },
            { State.RangeAttack,    new EnemyRobotState.RangeAttackState()  },
            { State.Death,          new EnemyRobotState.DeathState()        }
        };

        // 테이블 드리븐 전이 정의
        
    }

    private void Start()
    {
        

        transitions = new List<StateTransition<EnemyFSMBase>>() {
            new StateTransition<EnemyFSMBase>(State.Patrol, State.RangeAttack, e =>
                IsPlayerInSight(rangeAttackRange) || Vector3.Distance(transform.position, player.transform.position) < findRange),
            new StateTransition<EnemyFSMBase>(State.RangeAttack, State.MeleeAttack, e =>
                IsPlayerInSight(meleeAttackRange)),
            new StateTransition<EnemyFSMBase>(State.Patrol, State.MeleeAttack, e =>
                IsPlayerInSight(meleeAttackRange) || Vector3.Distance(transform.position, player.transform.position) < findRange),
            new StateTransition<EnemyFSMBase>(State.MeleeAttack, State.RangeAttack, e =>
                Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange),
            new StateTransition<EnemyFSMBase>(State.RangeAttack, State.Idle, e =>
                Vector3.Distance(transform.position, player.transform.position) > rangeAttackRange),

            new StateTransition<EnemyFSMBase>(State.ANY, State.Death, e =>
                currentHp <= 0)
        };

        transitionsByState = new Dictionary<State, List<StateTransition<EnemyFSMBase>>>();
        foreach (var t in transitions)
        {
            if (!transitionsByState.TryGetValue(t.From, out var list))
            {
                list = new List<StateTransition<EnemyFSMBase>>();
                transitionsByState[t.From] = list;
            }
            list.Add(t);
        }

        // ANY 전이도 따로 빼 두면 더 편리합니다
        if (!transitionsByState.ContainsKey(State.ANY))
            transitionsByState[State.ANY] = new List<StateTransition<EnemyFSMBase>>();

        // 초기 상태 설정
        CurrentState = State.Patrol;
        sm = new DataStateMachine<EnemyFSMBase>(this, stateMap[CurrentState]);
    }

    RaycastHit hit;
    public LayerMask obstacleMask;

    private bool IsPlayerInSight(float range)
    {
        // 1) 거리 체크
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > range)
            return false;

        // 2) FOV 체크
        Vector3 toPlayer = (player.transform.position + Vector3.up - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        Debug.DrawRay(transform.position, toPlayer * dist, Color.green, 0.1f);
        if (angle > viewAngle)
        {
            Debug.DrawRay(transform.position, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        // 3) Raycast 체크
        Vector3 origin = transform.position;
        Vector3 target = player.transform.position + Vector3.up;
        Vector3 dir = target - origin;
        Debug.DrawRay(origin, dir.normalized * range, Color.red, 0.1f);

        return IsRayHitOnPlayer(origin, dir);
    }

    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir)
    {

        // 1) 씬 뷰에 빨간 레이
        Debug.DrawRay(origin, dir.normalized * maxDist, Color.red, 0.1f);

        // 2) 실제 Raycast
        if (Physics.Raycast(origin, dir, out var hit, maxDist, obstacleMask))
        {
            //Debug.Log($"[Raycast] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            return hit;    // RaycastHit 반환
        }

        // 히트가 없으면 null
        return null;
    }

    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir)
    {
        // RaycastHit?이 RaycastHit hit으로 언랩핑되면 내부 블록 실행
        if (GetRaycastHit(origin, dir) is RaycastHit hit)
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }



    public void ChasePlayer()
    {
        agent.SetDestination(player.transform.position);
        agent.speed = 5f;
    }

    #region StateUpdate

    void Update()
    {
        // 1) 상태별 Update 호출
        sm.Update();

        // 2) 테이블 드리븐 전이 검사
        if (transitionsByState.TryGetValue(CurrentState, out var curList))
        {
            foreach (var t in curList)
            {
                if (t.Condition(this))
                {
                    ChangeState(t.To);
                    return;
                }
            }
        }

        // 2) ANY 전이 체크
        foreach (var t in transitionsByState[State.ANY])
        {
            if (t.Condition(this))
            {
                ChangeState(t.To);
                return;
            }
        }
    }

    void FixedUpdate()
    {
        sm.FixedUpdate();
    }

    // 전이 수행
    public void ChangeState(State next)
    {
        sm.ChangeState(stateMap[next]);
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
        rigid.AddForce(direction.normalized * force , ForceMode.Impulse);

        explodeOnWall = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground") && isDead)
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

    
}
