using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateInfo
{
    Idle,
    Attack,
    Move,
    Hit,
    Death,
}

public class EnemyRobot : EnemyBase
{
    
    BoxCollider _meleeAttackCollider;

    protected override void Start()
    {     
        base.Start();
        _meleeAttackCollider = GetComponentInChildren<BoxCollider>();
        _state = StateInfo.Idle;
    }

    


    protected override void FSM()
    {
        switch (_state)
        {
            case StateInfo.Idle:
                Idle();
            break;
            case StateInfo.Attack:
                Attack();
            break;
            case StateInfo.Move:
                Tick(RigidNavigation.MoveMode.Walk);

                if (IsPlayerInSight(_aggroRange) || IsCloseEnoughToPlayer())
                {
                    SetState(StateInfo.Idle);
                }
            break;
            case StateInfo.Hit:
                time += Time.deltaTime;
                if(time >= _hitTime)
                {
                    SetState(StateInfo.Idle);
                }
                float dir = Mathf.Sign(this.transform.position.x - _player.transform.position.x);
                // dir == +1 : 플레이어가 왼쪽에 있음 → 오른쪽으로 밀림
                // dir == -1 : 플레이어가 오른쪽에 있음 → 왼쪽으로 밀림

                Vector3 knockback = new Vector3(dir, 0, 0);
                transform.position += knockback * knockbackStrength * Time.deltaTime;
            break;

        }
    }

    #region StateChange
    protected override void SetState(StateInfo next)
    {
        ExitState(_state);
        _state = next;
        EnterState(_state);
    }

    protected override void EnterState(StateInfo cur)
    {
        switch (cur)
        {
            case StateInfo.Move:
                BeginPatrol(RigidNavigation.MoveMode.Walk);
                break;
            case StateInfo.Attack:
                // 필요하면 여기서 애니 초기 세팅
                break;
            case StateInfo.Hit:
                // 히트 시작 시 필요한 초기화
                time = 0f;
                break;
        }
    }

    protected override void ExitState(StateInfo cur)
    {
        switch (cur)
        {
            case StateInfo.Move:
                _nav.isStopped = true;
                break;
            case StateInfo.Attack:
                _meleeAttackCollider.enabled = false;  // 공격 끝나면 안전하게 끄기
                break;
            case StateInfo.Hit:
                time = 0f;                             // 히트 타이머 리셋
                break;
        }
    }

    private float knockbackStrength = 3;
    private float _hitTime = 0.3f;
    private float time = 0;
    private void Idle()
    {
        _animator.Play("Idle");
        if(IsPlayerInSight(_aggroRange) || IsCloseEnoughToPlayer())
        {
            SetState(StateInfo.Attack);
        }
        else
        {
            SetState(StateInfo.Move);
        }
    }
    #endregion


    private void Attack()
    {
        Debug.Log(!IsPlayerInSight(_aggroRange) && !IsCloseEnoughToPlayer());
        if (!IsPlayerInSight(_aggroRange) && !IsCloseEnoughToPlayer() && !IsInOrTransitionToAttack())
        {
            
            SetState(StateInfo.Move);
            return;
        }

        if (IsInOrTransitionToAttack()) return;
        
        _meleeAttackCollider.enabled = true;
        _animator.ResetTrigger("Attack");
        _animator.SetTrigger("Attack");
    }

    #region Helper
    private bool IsCloseEnoughToPlayer()
    {
        return Vector3.Distance(transform.position, _player.transform.position) < _findRange;
    }

    private bool IsInOrTransitionToAttack()
    {
        var st = _animator.GetCurrentAnimatorStateInfo(0);
        if (st.IsTag("Attack")) return true;

        if (_animator.IsInTransition(0))
        {
            var next = _animator.GetNextAnimatorStateInfo(0);
            if (next.IsTag("Attack")) return true;
        }
        return false;
    }

    public override void TakeDamage(in DamageInfo info)
    {
        base.TakeDamage(info);
        _state = StateInfo.Hit;
    }
    #endregion
}
