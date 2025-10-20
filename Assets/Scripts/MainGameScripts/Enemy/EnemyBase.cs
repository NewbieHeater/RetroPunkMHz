using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IAttackable, IExplosionInteract
{
    public int RequiredAmpPts => throw new System.NotImplementedException();

    public int RequiredPerPts => throw new System.NotImplementedException();

    public int RequiredWavPts => throw new System.NotImplementedException();

    [SerializeField] protected RigidPlayerManagement _player;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected RigidNavigation _nav;
    [SerializeField] protected PatrolController _patrolController;
    [SerializeField] protected AnimatorProxy _animatorProxy;

    [Header("점프 설정")]
    [Tooltip("포물선 최고점까지 높이")]
    [SerializeField] public float _defaultApexHeight = 2f;
    [Tooltip("왕복여부(False시 순환)")]
    [SerializeField] public bool _getBackAvailable = false;

    [Header("Ranges")]
    [SerializeField] protected float _findRange = 1.5f;
    [SerializeField] protected float _meleeAttackRange = 2f;
    [SerializeField] protected float _rangeAttackRange = 2f;
    [SerializeField] protected float _aggroRange = 5f;
    [SerializeField] protected float _attackRange = 5f;
    [SerializeField] protected float _maxDist = 5;

    [Header("시야 설정")]
    [Tooltip("에너미가 플레이어를 볼 수 있는 최대 각도(도)")]
    [SerializeField] protected float _viewAngle = 45f;
    [SerializeField] protected LayerMask _obstacleMask;
    protected StateInfo _state;
    public void OnExplosionInteract(Channel channel)
    {
        throw new System.NotImplementedException();
    }

    protected virtual void Start()
    {
        _player = GameManager.Instance.player;
        _animator = GetComponentInChildren<Animator>();
        _capsule = GetComponent<CapsuleCollider>();
        _rigid = GetComponent<Rigidbody>();
        _nav = GetComponent<RigidNavigation>();
        _animatorProxy = GetComponent<AnimatorProxy>();
        _patrolController = GetComponent<PatrolController>();
        _patrolController.Init(_nav, _animatorProxy);
    }

    protected void Update()
    {
        FSM();
    }

    protected abstract void SetState(StateInfo next);
    protected abstract void EnterState(StateInfo next);
    protected abstract void ExitState(StateInfo next);

    protected abstract void FSM();

    protected bool IsPlayerInSight(float range)
    {
        float dist = Vector3.Distance(transform.position, _player.transform.position);
        if (dist > range)
            return false;

        Vector3 toPlayer = (_player.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(_animator.transform.forward, toPlayer);
        Debug.DrawRay(transform.position + Vector3.up, toPlayer * dist, Color.green, 0.1f);
        if (angle > _viewAngle)
        {
            Debug.DrawRay(transform.position + Vector3.up, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        Vector3 origin = transform.position + Vector3.up;
        Vector3 target = _player.transform.position + Vector3.up;
        Vector3 dir = target - origin;
        Debug.DrawRay(origin, dir.normalized * range, Color.red, 0.1f);

        return IsRayHitOnPlayer(origin, dir);
    }

    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir, float distance)
    {
        Debug.DrawRay(origin, dir.normalized * distance, Color.red, 0.1f);

        if (Physics.Raycast(origin, dir, out var hit, distance, _obstacleMask))
        {
            return hit;
        }

        return null;
    }

    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir)
    {
        if (GetRaycastHit(origin, dir, _maxDist) is RaycastHit hit)
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }


    CapsuleCollider _capsule;
    Rigidbody _rigid;
    bool isDead;
    float _explosionRadius;

    #region knockback
    public void ApplyKnockback(Vector3 direction, float force)
    {
        _capsule.enabled = true;
        _rigid.velocity = Vector3.zero;
        _rigid.isKinematic = false;
        _rigid.useGravity = true;
        _rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigid.AddForce(direction.normalized * force / 1.3f, ForceMode.Impulse);
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
        var hits = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IExplosionInteract>(out var reactable))
                reactable.OnExplosionInteract(ChannelManager.Instance.CurrentChannel);
        }
        Destroy(gameObject);
    }
    private float mCurrentHp = 100;
    public virtual void TakeDamage(in DamageInfo info)
    {
        if (isDead) return;
        mCurrentHp -= info.Amount;
        
        Debug.Log("hit");
        if (mCurrentHp <= 0)
        {
            mCurrentHp = 0;
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
    }

    private void DieInstant()
    {
        isDead = true;
        gameObject.SetActive(false);
    }

    void IAttackable.TakeDamage(in DamageInfo info)
    {
        TakeDamage(info);
    }
    #endregion
}
