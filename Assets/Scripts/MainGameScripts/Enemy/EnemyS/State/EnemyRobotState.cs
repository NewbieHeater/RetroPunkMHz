using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;

namespace EnemyRobotState
{
    public class PatrolState : BaseState
    {
        public PatrolState(EnemyFSMBase enemy) : base(enemy)
        {

        }

        public override void OperateEnter()
        {
            enemy.enemyPatrolBaseInstance.OperateEnter();
        }

        public override void OperateUpdate()
        {
            enemy.enemyPatrolBaseInstance.OperateUpdate();
        }

        public override void OperateExit()
        {
            enemy.enemyPatrolBaseInstance.OperateExit();
        }

        public override void OperateFixedUpdate() 
        {
            enemy.enemyPatrolBaseInstance.OperateFixedUpdate();
        }    
    }

    public class MoveState : BaseState
    {
        public MoveState(EnemyFSMBase enemy) : base(enemy) { }
        public override void OperateEnter() { }
        public override void OperateUpdate() { }
        public override void OperateExit() { }
        public override void OperateFixedUpdate() { }
    }

    public class IdleState : BaseState
    {
        public IdleState(EnemyFSMBase enemy) : base(enemy) { }

        public override void OperateEnter()
        {
            enemy.enemyIdleBaseInstance.OperateEnter();
        }

        public override void OperateUpdate()
        {
            enemy.enemyIdleBaseInstance.OperateUpdate();
        }

        public override void OperateExit()
        {
            enemy.enemyIdleBaseInstance.OperateExit();
        }

        public override void OperateFixedUpdate() 
        {
            enemy.enemyIdleBaseInstance.OperateFixedUpdate();
        }
    }


    public class SearchState : BaseState
    {
        private float waitTime = 2f;
        private float curTime = 0f;

        public SearchState(EnemyFSMBase enemy) : base(enemy) { }

        public override void OperateEnter()
        {
            enemy.SetQuestionMark(true);
            enemy.anime.Play("Idle");
            curTime = 0f;
        }

        public override void OperateUpdate()
        {
            curTime += Time.deltaTime;
            if (curTime >= waitTime && enemy.patrolPoints.Length > 0)
                enemy.ChangeState(State.Idle);
        }

        public override void OperateExit()
        {
            enemy.SetQuestionMark(false);
            curTime = 0f;
        }

        public override void OperateFixedUpdate() { }
    }


    public class AttackState : BaseState
    {        

        public AttackState(EnemyFSMBase enemy) : base(enemy)
        {
        }

        public override void OperateEnter()
        {
            enemy.enemyAttackBaseInstance.OperateEnter();
        }

        public override void OperateUpdate()
        {
            enemy.enemyAttackBaseInstance.OperateUpdate();
        }

        public override void OperateExit()
        {
            enemy.enemyAttackBaseInstance.OperateExit();
        }

        public override void OperateFixedUpdate() 
        {
            enemy.enemyAttackBaseInstance.OperateFixedUpdate();
        }

        
    }



    public class RangeAttackState : BaseState
    {

        public RangeAttackState(EnemyFSMBase enemy) : base(enemy)
        {
        }

        public override void OperateEnter()
        {
            enemy.enemyAttackBaseInstance.OperateEnter();
        }

        public override void OperateUpdate()
        {
            enemy.enemyAttackBaseInstance.OperateUpdate();
        }

        public override void OperateExit()
        {
            enemy.enemyAttackBaseInstance.OperateExit();
        }

        public override void OperateFixedUpdate()
        {
            enemy.enemyAttackBaseInstance.OperateFixedUpdate();
        }


    }

    public class DeathState : BaseState
    {
        public DeathState(EnemyFSMBase enemy) : base(enemy) { }

        public override void OperateEnter()
        {
            enemy.isDead = true;
            enemy.agent.enabled = false;
        }

        public override void OperateUpdate()
        {
        }

        public override void OperateExit()
        {
        }

        public override void OperateFixedUpdate() { }
    }
}
public interface IFireable
{
    void Fire();
}