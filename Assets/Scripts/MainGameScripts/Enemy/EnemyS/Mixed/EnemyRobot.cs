using UnityEngine;

public class EnemyRobot : EnemyBase
{
    [SerializeField] private BoxCollider _meleeAttackCollider;
    [SerializeField] private float knockbackStrength = 3f;
    [SerializeField] private float hitStun = 0.3f;

    // 애니메이션 태그/트리거 이름(Animator 상태머신과 맞추세요)
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string moveState = "Move";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string attackTag = "Attack";
    [SerializeField] private string hitState = "Hit";
    [SerializeField] private string deathState = "Death";

    protected override void OnEnable()
    {
        base.OnEnable();
        SetState(StateInfo.Idle);
    }

    protected override void TickIdle()
    {
        _animator.Play(idleState);

        if (IsPlayerInSight(_aggroRange))
        {
            SetState(StateInfo.Attack);
        }
        else
        {
            SetState(StateInfo.Move);
        }
    }

    protected override void TickMove()
    {
        PatrolTick(RigidNavigation.MoveMode.Walk);

        if (IsPlayerInSight(_aggroRange))
            SetState(StateInfo.Idle); // 재판단
    }

    protected override void TickAttack()
    {
        // 플레이어를 잃었고, 공격 동작 중/전환중이 아니면 이동으로
        if (!IsPlayerInSight(_aggroRange) && !IsInOrTransitionToAttack())
        {
            SetState(StateInfo.Move);
            return;
        }

        if (IsInOrTransitionToAttack()) return;

        // 공격 발동
        ToggleMelee(true);
        _animator.ResetTrigger(attackTrigger);
        _animator.SetTrigger(attackTrigger);
    }

    protected override void TickHit()
    {
        
        float dir = (_player) ? Mathf.Sign(transform.position.x - _player.transform.position.x) : 1f;

        Vector3 kb = new Vector3(dir * knockbackStrength, 0f, 0f);
        transform.position += kb * Time.deltaTime;
        //if (_rigid) _rigid.velocity = new Vector3(kb.x, _rigid.velocity.y, 0f);
        //else transform.position += kb * Time.deltaTime;

        if (_stateTime >= hitStun)
        {
            SetState(StateInfo.Idle);
        }
    }

    protected override void OnEnterState(StateInfo s)
    {
        base.OnEnterState(s);
        if (s == StateInfo.Attack)
            ToggleMelee(false); // 안전
        if (s == StateInfo.Hit)
            _animator.Play(hitState, 0, 0f);
        if (s == StateInfo.Death)
            _nav.SetEnabled(false);
    }

    protected override void OnExitState(StateInfo s)
    {
        base.OnExitState(s);
        if (s == StateInfo.Move) _nav.isStopped = true;
        if (s == StateInfo.Attack) ToggleMelee(false);
    }

    public override void TakeDamage(in DamageInfo info)
    {
        base.TakeDamage(info);
        if (_state != StateInfo.Death)
            SetState(StateInfo.Hit);
    }

    bool IsInOrTransitionToAttack()
    {
        var st = _animator.GetCurrentAnimatorStateInfo(0);
        if (st.IsTag(attackTag)) return true;

        if (_animator.IsInTransition(0))
        {
            var next = _animator.GetNextAnimatorStateInfo(0);
            if (next.IsTag(attackTag)) return true;
        }
        return false;
    }

    void ToggleMelee(bool on)
    {
        if (_meleeAttackCollider && _meleeAttackCollider.enabled != on)
            _meleeAttackCollider.enabled = on;
    }
}
