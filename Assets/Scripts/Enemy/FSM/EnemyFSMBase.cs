using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using static UnityEngine.UI.Image;

public enum State { Patrol, Chase, Idle, Attack, Death, ANY }


[System.Serializable]
public struct PatrolPoint
{
    [Tooltip("������ ��ġ")]
    public Transform point;

    [Tooltip("�� ��ġ���� ��ٸ� �ð�(��)")]
    public float dwellTime;

    [Tooltip("�� ��ġ�� �̵��� �� ������ �ʿ��Ѱ�?")]
    public bool needJump;

    [Tooltip("���� ���� (needJump == true �� ����)")]
    public float jumpPower;
}

public abstract class EnemyFSMBase : MonoBehaviour//, IExplosionInteract, IKnockbackable, IAttackable
{
    [Header("���� ���� (ScriptableObject)")]
    [Tooltip("�����Ϳ��� PatrolBehaviorSO �Ҵ�")]
    public PatrolBehaviorSO patrolBehavior;

    [Header("���� ����")]
    [SerializeField] protected TextMeshProUGUI HpBar;
    [SerializeField] protected int maxHp = 100;
    protected int currentHp;
    protected bool isDead;

    [Header("���� ����")]
    [Tooltip("������ �ְ������� ���� (��������Ʈ�� jumpPower�� apex�� ���)")]
    public float defaultApexHeight = 2f;
    [Tooltip("�պ�����(False�� ��ȯ)")]
    public bool getBackAvailable = false;

    [Header("�˹�/���� ����")]
    [SerializeField] protected bool explodeOnWall;
    [SerializeField] protected float collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float explosionRadius = 3f;

    [Header("�þ� ����")]
    [Tooltip("���ʹ̰� �÷��̾ �� �� �ִ� �ִ� ����(��)")]
    public float viewAngle = 30f;

    // --- SO ���� ������ ���� (�� ���͸��� ���� ����) ---
    [HideInInspector] public int so_patrolIndex = 0;
    [HideInInspector] public bool so_isWaiting = false;
    [HideInInspector] public float so_waitStartTime = 0f;
    [HideInInspector] public bool so_isGoingForward = true;

    // ������Ʈ ����
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
    public Transform player;
    [Header("Ranges")]
    public float attackRange = 2f;
    public float aggroRange = 5f;
    float maxDist = 0;
    public PatrolPoint[] patrolPoints;

    protected virtual void Awake()
    {
        maxDist = aggroRange + 1f;

        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anime = GetComponent<Animator>();
        cap = GetComponent<CapsuleCollider>();
        //agent.updateRotation = false;
        currentHp = maxHp;
        UpdateHpBarText();

        // ���� �ν��Ͻ� ����
        stateMap = new Dictionary<State, IState<EnemyFSMBase>>()
        {
            { State.Patrol, new EnemyRobotState.PatrolState() },
            { State.Chase,   new EnemyRobotState.MoveState()   },
            { State.Idle,   new EnemyRobotState.IdleState()   },
            { State.Attack, new EnemyRobotState.AttackState() },
            { State.Death, new EnemyRobotState.DeathState() }
        };

        // ���̺� �帮�� ���� ����
        
    }

    private void Start()
    {
        patrolBehavior = Instantiate(patrolBehavior);
        patrolBehavior.Initialize(gameObject, this);

        transitions = new List<StateTransition<EnemyFSMBase>>() {
            // Patrol -> Chase
            //new StateTransition<EnemyFSMBase>(State.Patrol, State.Chase, e =>
            //    IsPlayerChaseAble()),
            //// Chase -> Attack
            //new StateTransition<EnemyFSMBase>(State.Chase, State.Attack, e =>
            //    Vector3.Distance(e.transform.position, e.player.position) <= e.attackRange),
            //// Attack -> Chase
            //new StateTransition<EnemyFSMBase>(State.Attack, State.Chase, e =>
            //{
            //    float d = Vector3.Distance(e.transform.position, e.player.position);
            //    return d > e.attackRange && d <= e.aggroRange;
            //}),
            //// Chase -> Patrol
            //new StateTransition<EnemyFSMBase>(State.Chase, State.Patrol, e =>
            //    Vector3.Distance(e.transform.position, e.player.position) > e.aggroRange),
            new StateTransition<EnemyFSMBase>(State.Patrol, State.Attack, e =>
                IsPlayerInSight(attackRange)),
            new StateTransition<EnemyFSMBase>(State.Attack, State.Idle, e =>
                !IsPlayerInSight(attackRange)),
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

        // ANY ���̵� ���� �� �θ� �� ���մϴ�
        if (!transitionsByState.ContainsKey(State.ANY))
            transitionsByState[State.ANY] = new List<StateTransition<EnemyFSMBase>>();

        // �ʱ� ���� ����
        CurrentState = State.Patrol;
        sm = new DataStateMachine<EnemyFSMBase>(this, stateMap[CurrentState]);
    }

    RaycastHit hit;
    public LayerMask obstacleMask;

    /// <summary>
    /// �÷��̾ �־��� range �ȿ� �ְ�, �þ߰� �ȿ� ������,
    /// Raycast �� ������ �ʴ���(��ֹ� ���� ���̴���)���� ��� �˻��մϴ�.
    /// </summary>
    private bool IsPlayerInSight(float range)
    {
        // 1) �Ÿ� üũ
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > range)
            return false;

        // 2) FOV üũ
        Vector3 toPlayer = (player.position + Vector3.up - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        Debug.DrawRay(transform.position, toPlayer * dist, Color.green, 0.1f);
        if (angle > viewAngle)
        {
            Debug.DrawRay(transform.position, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        // 3) Raycast üũ
        Vector3 origin = transform.position;
        Vector3 target = player.position + Vector3.up;
        Vector3 dir = target - origin;
        Debug.DrawRay(origin, dir.normalized * range, Color.red, 0.1f);

        return IsRayHitOnPlayer(origin, dir);
    }


    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir)
    {

        // 1) �� �信 ���� ����
        Debug.DrawRay(origin, dir.normalized * maxDist, Color.red, 0.1f);

        // 2) ���� Raycast
        if (Physics.Raycast(origin, dir, out var hit, maxDist, obstacleMask))
        {
            Debug.Log($"[Raycast] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            return hit;    // RaycastHit ��ȯ
        }

        // ��Ʈ�� ������ null
        return null;
    }

    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir)
    {
        // RaycastHit?�� RaycastHit hit���� ���εǸ� ���� ��� ����
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
        // 1) ���º� Update ȣ��
        sm.Update();

        // 2) ���̺� �帮�� ���� �˻�
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

        // 2) ANY ���� üũ
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

    // ���� ����
    private void ChangeState(State next)
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

    #region ������ó��
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
