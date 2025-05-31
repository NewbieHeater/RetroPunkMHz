using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyRobotState
{
    public class PatrolState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) 
        {
            e.patrolBehavior.DoEnterLogic();
        }
        public void OperateExit(EnemyFSMBase e) { /* 정리 */ }
        public void OperateUpdate(EnemyFSMBase e) { e.patrolBehavior.DoUpdateLogic(); }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }

    public class MoveState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) {  }
        public void OperateExit(EnemyFSMBase e) 
        { 
            
        }
        public void OperateUpdate(EnemyFSMBase e) { e.ChasePlayer(); }
        public void OperateFixedUpdate(EnemyFSMBase e) {  }
    }

    public class IdleState : IState<EnemyFSMBase>
    {
        //나중에 수정할게요 귀차나
        
        public void OperateEnter(EnemyFSMBase e) 
        {
            e.idleBehavior.DoEnterLogic();
        }
        public void OperateExit(EnemyFSMBase e) { e.idleBehavior.DoExitLogic(); }
        public void OperateUpdate(EnemyFSMBase e) 
        {
            e.idleBehavior.DoUpdateLogic();
        }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }

    public class AttackState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) 
        { 
            e.agent.isStopped = true;
            e.agent.velocity = Vector3.zero;
            e.meleeAttackBehavior.DoEnterLogic();
        }
        public void OperateExit(EnemyFSMBase e) 
        {
            e.meleeAttackBehavior.DoExitLogic();
        }
        public void OperateUpdate(EnemyFSMBase e) { e.meleeAttackBehavior.DoUpdateLogic(); }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }

    public class RangeAttackState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e)
        {
            e.agent.isStopped = true;
            e.agent.velocity = Vector3.zero;
            e.rangeAttackBehavior.DoEnterLogic();
        }
        public void OperateExit(EnemyFSMBase e)
        {
            e.rangeAttackBehavior.DoExitLogic();
        }
        public void OperateUpdate(EnemyFSMBase e) { e.rangeAttackBehavior.DoUpdateLogic(); }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }

    public class DeathState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) 
        { 
            e.agent.enabled = false;
        }
        public void OperateExit(EnemyFSMBase e) { }
        public void OperateUpdate(EnemyFSMBase e) { /* 공격 로직 */ }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }
}