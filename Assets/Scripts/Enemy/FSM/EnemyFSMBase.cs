using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

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

public abstract class EnemyFSMBase : MonoBehaviour, IExplosionInteract, IKnockbackable, IAttackable
{
    [Header("���� ����")]
    [SerializeField] protected TextMeshProUGUI  HpBar;
    [SerializeField] protected int              maxHp = 100;
    protected int currentHp;
    protected bool isDead;
    [Header("���� ����")]
    [Tooltip("������ �ְ������� ���� (��������Ʈ�� jumpPower�� apex�� ���)")]
    public float defaultApexHeight = 2f;
    [Tooltip("�պ�����(False�� ��ȯ)")]
    public bool getBackAvailable = false;

    [Header("�˹�/���� ����")]
    [SerializeField] protected bool     explodeOnWall;
    [SerializeField] protected float    collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float    explosionRadius = 3f;

    [Header("���� ��� ����")]
    [Tooltip("������ �������� Inspector���� �巡���ϼ���.")]
    [SerializeField] public float patrolSpeed = 5f;
    [SerializeField] public PatrolPoint[] patrolPoints;

    [Header("�þ� ����")]
    [Tooltip("���ʹ̰� �÷��̾ �� �� �ִ� �ִ� ����(��)")]
    public float viewAngle = 30f;  // ��30�� �ȿ����� �÷��̾� �ν�

    protected bool  isWaiting;
    protected float waitStartTime;
    public int     patrolIndex;
    private bool    isGoingForward = true;

    public virtual int RequiredAmpPts => 0;
    public virtual int RequiredPerPts => 0;
    public virtual int RequiredWavPts => 0;


    protected CapsuleCollider   cap;
    protected Rigidbody         rigid { get; private set; }
    public NavMeshAgent      agent { get; private set; }
    protected Animator          anime { get; private set; }

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

    protected virtual void Awake()
    {
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
        transitions = new List<StateTransition<EnemyFSMBase>>() {
            // Patrol -> Chase
            new StateTransition<EnemyFSMBase>(State.Patrol, State.Chase, e =>
                IsPlayerChaseAble()),
            // Chase -> Attack
            new StateTransition<EnemyFSMBase>(State.Chase, State.Attack, e =>
                Vector3.Distance(e.transform.position, e.player.position) <= e.attackRange),
            // Attack -> Chase
            new StateTransition<EnemyFSMBase>(State.Attack, State.Chase, e =>
            {
                float d = Vector3.Distance(e.transform.position, e.player.position);
                return d > e.attackRange && d <= e.aggroRange;
            }),
            // Chase -> Patrol
            new StateTransition<EnemyFSMBase>(State.Chase, State.Patrol, e =>
                Vector3.Distance(e.transform.position, e.player.position) > e.aggroRange),

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
    public LayerMask playerLayer;
    public bool IsPlayerChaseAble()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > aggroRange) return false;

        // 2) �þ�(FOV) üũ
        Vector3 toPlayer = (player.position + Vector3.up - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        Debug.DrawRay(transform.position, toPlayer * dist, Color.green, 0.1f);
        if (angle > viewAngle)
        {
            // Debug.DrawRay�� �þ� ������ �ð�ȭ
            Debug.DrawRay(transform.position, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        Vector3 origin = transform.position;
        Vector3 target = player.position + Vector3.up;
        Vector3 dir = target - origin;
        float maxDist = aggroRange + 1f;

        // 1) �� �信 ���� ����(Debug.DrawRay)
        Debug.DrawRay(origin, dir.normalized * maxDist, Color.red, 0.1f);

        // 2) ���� Raycast
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, playerLayer))
        {
            // 3) ��Ʈ�� �ݶ��̴� �̸��� �±� �α�
            Debug.Log($"[IsPlayerChaseAble] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            return hit.collider.CompareTag("Player");
        }

        // 4) ��Ʈ �� ���� �� �α�
        Debug.Log("[IsPlayerChaseAble] Raycast did not hit anything within range.");
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

    #region Patrol
    public void PatrolerManual()
    {
        Debug.Log("isPatrol");
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        var pt = patrolPoints[patrolIndex];

        // 1) ��� �� ó��
        if (isWaiting)
        {
            if (Time.time - waitStartTime >= pt.dwellTime)
            {
                isWaiting = false;
                AdvancePatrolIndex();
                agent.SetDestination(patrolPoints[patrolIndex].point.position);
            }
            return;
        }

        // 2) ���� ��ΰ� ������ ������ ����
        //if (!agent.hasPath && !agent.pathPending)
        //{
        //    agent.SetDestination(pt.point.position);
        //}

        // 3) ���� ���� (X �� ���������)
        if (!agent.pathPending
            && Mathf.Abs(pt.point.position.x - transform.position.x) < 0.05f && agent.enabled)
        {
            // �̵� ���� ����
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            // --- ���� ���� ---
            if (pt.needJump && isGoingForward)
            {
                agent.updatePosition = false;
                agent.enabled = false;
                cap.enabled = true;
                // 2) ���� �ε��� �̸� ���
                AdvancePatrolIndex();
                var nextPos = patrolPoints[patrolIndex].point.position;

                // 3) �ʱ� �ӵ� ���
                float apexH = pt.jumpPower > 0 ? pt.jumpPower : defaultApexHeight;
                Vector3 launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);

                // 4) ���� ����
                rigid.useGravity = true;
                rigid.velocity = launch;

                // 5) ������ ����
                pt.needJump = false;
                //patrolPoints[patrolIndex] = pt;

                // 6) ���� �� ����
                StartCoroutine(WaitForLandingAndResume(nextPos));
                Debug.Log(patrolIndex);
                return;
            }

            // --- ��� �Ǵ� �ٷ� ���� ���� ---
            if (pt.dwellTime > 0f)
            {
                isWaiting = true;
                waitStartTime = Time.time;
            }
            else
            {
                AdvancePatrolIndex();
                agent.SetDestination(patrolPoints[patrolIndex].point.position);
            }
        }

    }
    private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float apexHeight)
    {
        float g = Physics.gravity.y;
        // ��� �ӵ�
        float vUp = Mathf.Sqrt(-2f * g * apexHeight);
        float tUp = vUp / -g;
        // ������ �� �ɸ��� �ð�
        float deltaH = apexHeight - (end.y - start.y);
        float tDown = Mathf.Sqrt(2f * deltaH / -g);
        float totalT = tUp + tDown;

        // ���� ���� (3D)
        Vector3 horiz = end - start;
        horiz.y = 0f;
        Vector3 vHoriz = horiz / totalT;

        return vHoriz + Vector3.up * vUp;
    }

    private IEnumerator WaitForLandingAndResume(Vector3 resumeTarget)
    {

        yield return new WaitForSeconds(2);

        rigid.velocity = Vector3.zero;
        cap.enabled = false;
        // Agent �ٽ� �� �ֱ�
        agent.enabled = true;
        agent.updatePosition = true;
        agent.isStopped = false;

        agent.SetDestination(patrolPoints[patrolIndex].point.position);
    }

    // ping-pong �ε��� ���
    private void AdvancePatrolIndex()
    {
        if (getBackAvailable)
        {
            if (isGoingForward)
            {
                patrolIndex++;
                if (patrolIndex >= patrolPoints.Length)
                {
                    patrolIndex = patrolPoints.Length - 2;
                    isGoingForward = false;
                }
            }
            else
            {
                patrolIndex--;
                if (patrolIndex < 0)
                {
                    patrolIndex = 1;
                    isGoingForward = true;
                }
            }
        }
        else
        {
            patrolIndex++;
            if (patrolIndex >= patrolPoints.Length)
            {
                patrolIndex = 0;
            }
        }
    }


    #endregion
}
