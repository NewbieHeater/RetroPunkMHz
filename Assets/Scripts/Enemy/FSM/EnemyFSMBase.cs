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
    [Tooltip("순찰할 위치")]
    public Transform point;

    [Tooltip("이 위치에서 기다릴 시간(초)")]
    public float dwellTime;

    [Tooltip("이 위치로 이동할 때 점프가 필요한가?")]
    public bool needJump;

    [Tooltip("점프 높이 (needJump == true 일 때만)")]
    public float jumpPower;
}

public abstract class EnemyFSMBase : MonoBehaviour, IExplosionInteract, IKnockbackable, IAttackable
{
    [Header("스텟 설정")]
    [SerializeField] protected TextMeshProUGUI  HpBar;
    [SerializeField] protected int              maxHp = 100;
    protected int currentHp;
    protected bool isDead;
    [Header("점프 설정")]
    [Tooltip("포물선 최고점까지 높이 (웨이포인트별 jumpPower를 apex로 사용)")]
    public float defaultApexHeight = 2f;
    [Tooltip("왕복여부(False시 순환)")]
    public bool getBackAvailable = false;

    [Header("넉백/폭발 설정")]
    [SerializeField] protected bool     explodeOnWall;
    [SerializeField] protected float    collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float    explosionRadius = 3f;

    [Header("순찰 경로 설정")]
    [Tooltip("순찰할 지점들을 Inspector에서 드래그하세요.")]
    [SerializeField] public float patrolSpeed = 5f;
    [SerializeField] public PatrolPoint[] patrolPoints;

    [Header("시야 설정")]
    [Tooltip("에너미가 플레이어를 볼 수 있는 최대 각도(도)")]
    public float viewAngle = 30f;  // ±30° 안에서만 플레이어 인식

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

        // 상태 인스턴스 매핑
        stateMap = new Dictionary<State, IState<EnemyFSMBase>>()
        {
            { State.Patrol, new EnemyRobotState.PatrolState() },
            { State.Chase,   new EnemyRobotState.MoveState()   },
            { State.Idle,   new EnemyRobotState.IdleState()   },
            { State.Attack, new EnemyRobotState.AttackState() },
            { State.Death, new EnemyRobotState.DeathState() }
        };

        // 테이블 드리븐 전이 정의
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

        // ANY 전이도 따로 빼 두면 더 편리합니다
        if (!transitionsByState.ContainsKey(State.ANY))
            transitionsByState[State.ANY] = new List<StateTransition<EnemyFSMBase>>();

        // 초기 상태 설정
        CurrentState = State.Patrol;
        sm = new DataStateMachine<EnemyFSMBase>(this, stateMap[CurrentState]);
    }
    RaycastHit hit;
    public LayerMask playerLayer;
    public bool IsPlayerChaseAble()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > aggroRange) return false;

        // 2) 시야(FOV) 체크
        Vector3 toPlayer = (player.position + Vector3.up - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        Debug.DrawRay(transform.position, toPlayer * dist, Color.green, 0.1f);
        if (angle > viewAngle)
        {
            // Debug.DrawRay로 시야 밖임을 시각화
            Debug.DrawRay(transform.position, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        Vector3 origin = transform.position;
        Vector3 target = player.position + Vector3.up;
        Vector3 dir = target - origin;
        float maxDist = aggroRange + 1f;

        // 1) 씬 뷰에 빨간 레이(Debug.DrawRay)
        Debug.DrawRay(origin, dir.normalized * maxDist, Color.red, 0.1f);

        // 2) 실제 Raycast
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, playerLayer))
        {
            // 3) 히트된 콜라이더 이름과 태그 로깅
            Debug.Log($"[IsPlayerChaseAble] Hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            return hit.collider.CompareTag("Player");
        }

        // 4) 히트 못 했을 때 로깅
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

    #region Patrol
    public void PatrolerManual()
    {
        Debug.Log("isPatrol");
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        var pt = patrolPoints[patrolIndex];

        // 1) 대기 중 처리
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

        // 2) 아직 경로가 없으면 목적지 설정
        //if (!agent.hasPath && !agent.pathPending)
        //{
        //    agent.SetDestination(pt.point.position);
        //}

        // 3) 도착 판정 (X 축 가까워지면)
        if (!agent.pathPending
            && Mathf.Abs(pt.point.position.x - transform.position.x) < 0.05f && agent.enabled)
        {
            // 이동 완전 정지
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            // --- 점프 로직 ---
            if (pt.needJump && isGoingForward)
            {
                agent.updatePosition = false;
                agent.enabled = false;
                cap.enabled = true;
                // 2) 다음 인덱스 미리 계산
                AdvancePatrolIndex();
                var nextPos = patrolPoints[patrolIndex].point.position;

                // 3) 초기 속도 계산
                float apexH = pt.jumpPower > 0 ? pt.jumpPower : defaultApexHeight;
                Vector3 launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);

                // 4) 점프 실행
                rigid.useGravity = true;
                rigid.velocity = launch;

                // 5) 재점프 방지
                pt.needJump = false;
                //patrolPoints[patrolIndex] = pt;

                // 6) 착지 후 복귀
                StartCoroutine(WaitForLandingAndResume(nextPos));
                Debug.Log(patrolIndex);
                return;
            }

            // --- 대기 또는 바로 다음 지점 ---
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
        // 상승 속도
        float vUp = Mathf.Sqrt(-2f * g * apexHeight);
        float tUp = vUp / -g;
        // 내려올 때 걸리는 시간
        float deltaH = apexHeight - (end.y - start.y);
        float tDown = Mathf.Sqrt(2f * deltaH / -g);
        float totalT = tUp + tDown;

        // 수평 벡터 (3D)
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
        // Agent 다시 켜 주기
        agent.enabled = true;
        agent.updatePosition = true;
        agent.isStopped = false;

        agent.SetDestination(patrolPoints[patrolIndex].point.position);
    }

    // ping-pong 인덱스 계산
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
