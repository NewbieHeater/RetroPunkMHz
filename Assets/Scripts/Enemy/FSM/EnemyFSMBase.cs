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
    [Tooltip("������ ��ġ")]
    public Transform point;

    [Tooltip("�� ��ġ���� ��ٸ� �ð�(��)")]
    public float dwellTime;

    [Tooltip("�� ��ġ�� �̵��� �� ������ �ʿ��Ѱ�?")]
    public bool needJump;

    [Tooltip("���� ���� (needJump == true �� ����)")]
    public float jumpPower;
}

public abstract class EnemyFSMBase<TSelf> : MonoBehaviour,
    IAttackable, IExplosionInteract
    where TSelf : EnemyFSMBase<TSelf>
{
    public bool stop;
    [Header("���� ����")]
    [SerializeField] protected TextMeshProUGUI HpBar;
    [SerializeField] protected int          maxHp = 100;
    [SerializeField] protected int          currentHp;
    protected bool                          isDead;

    [Header("���� ����")]
    [Tooltip("������ �ְ������� ����")]
    [SerializeField] public float        defaultApexHeight = 2f;
    [Tooltip("�պ�����(False�� ��ȯ)")]
    [SerializeField] public bool         getBackAvailable = false;

    [Header("�˹�/���� ����")]
    [SerializeField] protected bool         explodeOnWall;
    [SerializeField] protected float        collisionDetectionSpeedThreshold = 5f;
    [SerializeField] protected float        explosionRadius = 3f;

    [Header("�þ� ����")]
    [Tooltip("���ʹ̰� �÷��̾ �� �� �ִ� �ִ� ����(��)")]
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

    public virtual int RequiredAmpPts => 0;
    public virtual int RequiredPerPts => 0;
    public virtual int RequiredWavPts => 0;

    public State CurrentState { get; protected set; }

    protected DataStateMachine<TSelf> fsm;
    protected Dictionary<State, BaseState<TSelf>> stateMap;
    protected List<StateTransition<TSelf>> transitions;

    public PlayerManagement player;


    protected virtual void Awake()
    {
        maxDist = aggroRange + 1f;

        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();
        cap   = GetComponent<CapsuleCollider>();
        
        currentHp = maxHp;
        UpdateHpBarText();
        
    }

    protected virtual void Start()
    {
        //player = GameManager.Instance.player;
        //fsm = new DataStateMachine<TSelf>(CurrentState, stateMap, transitions);
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
