using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyRobotState
{
    public class PatrolState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) 
        { 
            e.agent.speed = e.patrolSpeed;

            if (e.patrolPoints != null && e.patrolPoints.Length > 0)
            {
                Debug.Log(e.patrolPoints[e.patrolIndex].point.position);
                Debug.Log("PatrolStart");
                e.agent.SetDestination(
                    e.patrolPoints[e.patrolIndex].point.position
                );
            }
        }
        public void OperateExit(EnemyFSMBase e) { /* 정리 */ }
        public void OperateUpdate(EnemyFSMBase e) { e.PatrolerManual(); }
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
        public void OperateEnter(EnemyFSMBase e) { Debug.Log("IOdle"); }
        public void OperateExit(EnemyFSMBase e) { }
        public void OperateUpdate(EnemyFSMBase e) { /* 대기 중 애니메이션 */ }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }

    public class AttackState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) 
        { 
            Debug.Log("Attack");
            e.agent.isStopped = true;
            e.agent.velocity = Vector3.zero;
        }
        public void OperateExit(EnemyFSMBase e) 
        {
            e.agent.isStopped = false;
        }
        public void OperateUpdate(EnemyFSMBase e) { /* 공격 로직 */ }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }

    public class DeathState : IState<EnemyFSMBase>
    {
        public void OperateEnter(EnemyFSMBase e) 
        { 
            Debug.Log("Attack"); 
            e.agent.enabled = false;
        }
        public void OperateExit(EnemyFSMBase e) { }
        public void OperateUpdate(EnemyFSMBase e) { /* 공격 로직 */ }
        public void OperateFixedUpdate(EnemyFSMBase e) { }
    }
}