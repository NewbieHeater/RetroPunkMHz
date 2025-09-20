using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateInfo
{
    Idle,
    Attack,
    Move
}

public class EnemyRobot : EnemyBase
{
    StateInfo _state;
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
                _patrolController.Tick();
                if (IsPlayerInSight(_aggroRange))
                {
                    _patrolController.Exit();
                    _state = StateInfo.Attack;
                }
                break;
        }
    }

    private void Idle()
    {
        
        if(IsPlayerInSight(_aggroRange) || CloseEnoughToPlayer())
        {
            _state = StateInfo.Attack;
        }
        else
        {
            _state = StateInfo.Move;
        }
    }

    bool _isAttacking = false;

    private void Attack()
    {
        if (IsInOrTransitionToAttack()) return;

        // 이제 막 공격 시작
        //_isAttacking = true;
        _meleeAttackCollider.enabled = true;
        _animator.ResetTrigger("Attack"); // 혹시 남아있을지 몰라 안전
        _animator.SetTrigger("Attack");
    }

    #region Helper
    private bool CloseEnoughToPlayer()
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
    #endregion
}
