using UnityEngine.AI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEditor;

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

public enum State
{
    Patrol,
    Idle,
    Move,
    Attack,
}

public abstract class EnemyBase : MonoBehaviour, IExplosionInteract, IKnockbackable, IAttackable
{
    

    State _curState;

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

    [Header("순찰 경로 설정")]
    [Tooltip("순찰할 지점들을 Inspector에서 드래그하세요.")]
    [SerializeField] public PatrolPoint[] patrolPoints;
    [SerializeField] protected float patrolSpeed = 5f;
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

    public Dictionary<string, IState<EnemyBase>> dicState = new Dictionary<string, IState<EnemyBase>>();
    public StateMachine<EnemyBase> sm;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anime = GetComponent<Animator>();

        //rigid.useGravity = false;

        dicState.Add("Attack", new EnemyAttackState());
        dicState.Add("Move", new EnemyMoveState());
        dicState.Add("Idle", new EnemyIdleState());
        dicState.Add("Patrol", new EnemyPatrollState());
        sm = new StateMachine<EnemyBase>(this, dicState["Patrol"]);
        
        currentHp = maxHp;
        patrolIndex = 0;
        isGoingForward = true;
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
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            if (Time.time - waitStartTime >= patrolPoints[patrolIndex].dwellTime)
            {
                isWaiting = false;
                SetNextDestination();
                Debug.Log("next");
            }
            return;
        }

        Vector3 target = patrolPoints[patrolIndex].point.position;

        if (patrolPoints[patrolIndex].needJump)
        {

            return;
        }

        agent.SetDestination(target);

        if (Mathf.Abs(transform.position.x - target.x) <= 0.1f)
        {
            isWaiting = true;
            waitStartTime = Time.time;
            Debug.Log(transform.position.x - target.x);
        }
    }

    private void SetNextDestination()
    {
        // ping-pong 인덱스 계산
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
        rigid.velocity = Vector3.zero;
        rigid.isKinematic = false;
        rigid.useGravity = true;
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigid.AddForce(direction.normalized * force, ForceMode.Impulse);

        explodeOnWall = true;
    }
}
