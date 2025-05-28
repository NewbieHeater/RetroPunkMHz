using UnityEngine.AI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using static Unity.Burst.Intrinsics.X86.Avx;





public abstract class EnemyBase : MonoBehaviour, IExplosionInteract, IKnockbackable, IAttackable
{
    #region 변수
    [Header("스텟 설정")]
    [SerializeField] protected TextMeshProUGUI  HpBar;
    [SerializeField] protected int              maxHp = 100;
    protected int           currentHp;
    protected bool          isDead;
    

    [Header("넉백/폭발 설정")]
    [SerializeField] protected bool   explodeOnWall;
    [SerializeField] protected float  collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float  explosionRadius = 3f;

    [Header("점프 설정")]
    [Tooltip("포물선 최고점까지 높이 (웨이포인트별 jumpPower를 apex로 사용)")]
    public float defaultApexHeight = 2f;
    [Tooltip("왕복여부(False시 순환)")]
    public bool getBackAvailable = false;

    [Header("순찰 경로 설정")]
    [Tooltip("순찰할 지점들을 Inspector에서 드래그하세요.")]
    [SerializeField] protected float patrolSpeed = 5f;
    [SerializeField] public PatrolPoint[] patrolPoints;
    

    

    protected bool isWaiting;
    protected float waitStartTime;

    //채널 요구치
    public virtual int  RequiredAmpPts => 0;
    public virtual int  RequiredPerPts => 0;
    public virtual int  RequiredWavPts => 0;
    #endregion 

    public Rigidbody    rigid { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public Animator     anime { get; private set; }

    public Dictionary<State, IState<EnemyBase>> dicState = new Dictionary<State, IState<EnemyBase>>();
    public StateMachine<EnemyBase> sm;
    public State CurrentStateEnum { get; private set; }

    public CapsuleCollider cap;
    protected virtual void Awake()
    {

        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anime = GetComponent<Animator>();
        cap = GetComponent<CapsuleCollider>();
        agent.updateRotation = false;
        //rigid.useGravity = false;
        cap.enabled = false;
        dicState.Add(State.MeleeAttack, new EnemyAttackState());
        dicState.Add(State.Chase, new EnemyMoveState());
        dicState.Add(State.Idle, new EnemyIdleState());
        dicState.Add(State.Patrol, new EnemyPatrollState());
        sm = new StateMachine<EnemyBase>(this, dicState[State.Patrol]);
        CurrentStateEnum = State.Patrol;

        currentHp = maxHp;
        patrolIndex = 0;
        isGoingForward = true;
        UpdateHpBarText();
        agent.SetDestination(patrolPoints[0].point.position);
    }

    private void Update()
    {
        
        sm.DoOperateUpdate();
    }

    private void FixedUpdate()
    {
        sm.DoOperateFixedUpdate();
    }

    private int patrolIndex;
    private bool isGoingForward = true;


    
    #region Patrol
    public void PatrolerManual()
    {
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
        Debug.Log(patrolIndex);
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
        Debug.Log(patrolIndex + "waitdafsssssssssssssssss");
        // Rigidbody 중력은 계속 유지하거나, 필요 시 다시 꺼주세요
        // rigid.useGravity = false; // if 원상 복구 필요하면
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

    // 추상메서드
    public abstract void Patrol();
    public abstract void Attack();


    // 공통메서드(수정가능한)
    #region 데미지처리
    // — 일반 데미지 처리 (차지 공격이 아닐 때) —
    public void TakeDamage(in DamageInfo info)
    {
        if (isDead) return;

        currentHp -= info.Amount;

        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;

            // 1) 네비/애니 중지
            if (agent != null) agent.enabled = false;
            if (anime != null) anime.enabled = false;

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
        // HpBar 초깃값 할당을 Awake/Start에서 잊지 마세요.
        HpBar.text = $"Hp : {currentHp}";
    }

    private void DieInstant()
    {
        Destroy(gameObject);
    }
    #endregion

    #region 벽/바닥 충돌 감지 로직, 폭팔로직
    // — 벽/바닥 충돌 감지 로직 —
    public void OnCollisionEnter(Collision collision)
    {
        if (explodeOnWall && collision.collider.CompareTag("Ground"))
        {
            Explode();
            return;
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

    public virtual void OnExplosionInteract(Channel channel)
    {

    }
    #endregion

    public void ApplyKnockback(Vector3 direction, float force)
    {
        cap.enabled = true;
        rigid.velocity = Vector3.zero;
        rigid.isKinematic = false;
        rigid.useGravity = true;
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigid.AddForce(direction.normalized * force, ForceMode.Impulse);

        explodeOnWall = true;
    }
}
