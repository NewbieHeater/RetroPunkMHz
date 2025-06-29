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
        private Transform transform;
        private NavMeshAgent agent;
        private Rigidbody enemyRigid;
        private CapsuleCollider enemyCap;
        private int patrolIndex;
        private bool isWaiting;
        private float waitStartTime;
        private bool isGoingForward;
        public float patrolSpeed = 2f;

        public PatrolState(EnemyFSMBase enemy) : base(enemy)
        {
            transform = enemy.transform;
            agent = enemy.agent;
            enemyRigid = enemy.rigid;
            enemyCap = enemy.cap;
            patrolIndex = 0;
            isWaiting = false;
            isGoingForward = true;
        }

        public override void OperateEnter()
        {
            enemy.anime.Play("Move");
            agent.isStopped = false;
            agent.speed = patrolSpeed;

            if (enemy.patrolPoints == null || enemy.patrolPoints.Length <= 1)
            {
                if (enemy.patrolPoints != null && enemy.patrolPoints.Length == 1)
                    agent.SetDestination(enemy.patrolPoints[0].point.position);
                return;
            }

            if (!agent.hasPath)
                agent.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
        }

        public override void OperateUpdate()
        {
            if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0 || stop)
                return;

            if (enemy.patrolPoints.Length == 1)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    enemy.stop = true;
                    agent.isStopped = true;
                    enemy.ChangeState(State.Idle);
                }
                return;
            }

            if (isWaiting)
            {
                if (Time.time - waitStartTime >= enemy.patrolPoints[patrolIndex].dwellTime)
                {
                    isWaiting = false;
                    AdvanceIndex();
                    agent.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
                }
            }
            else
            {
                if (Mathf.Abs(enemy.patrolPoints[patrolIndex].point.position.x - transform.position.x) < 0.05f)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;

                    if (enemy.patrolPoints[patrolIndex].needJump && isGoingForward)
                    {
                        agent.ResetPath();
                        agent.enabled = false;
                        enemyCap.enabled = true;
                        float apexH = enemy.patrolPoints[patrolIndex].jumpPower > 0
                                      ? enemy.patrolPoints[patrolIndex].jumpPower
                                      : enemy.defaultApexHeight;
                        AdvanceIndex();
                        Vector3 nextPos = enemy.patrolPoints[patrolIndex].point.position;
                        Vector3 launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);
                        enemyRigid.useGravity = true;
                        enemyRigid.velocity = launch;
                        enemy.patrolPoints[patrolIndex].needJump = false;
                        stop = true;
                        enemy.StartCoroutine(ResumeAfterJump(nextPos));
                    }
                    else
                    {
                        if (enemy.patrolPoints[patrolIndex].dwellTime > 0f)
                        {
                            isWaiting = true;
                            waitStartTime = Time.time;
                        }
                        else
                        {
                            AdvanceIndex();
                            agent.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
                        }
                    }
                }
            }
        }

        public override void OperateExit()
        {
            agent.isStopped = true;
        }

        public override void OperateFixedUpdate() { }

        private void AdvanceIndex()
        {
            int len = enemy.patrolPoints.Length;
            if (len <= 1)
            {
                patrolIndex = 0;
                isWaiting = false;
                return;
            }

            if (enemy.getBackAvailable)
            {
                if (isGoingForward)
                {
                    patrolIndex++;
                    if (patrolIndex >= len)
                    {
                        patrolIndex = len - 2;
                        isGoingForward = false;
                    }
                }
                else
                {
                    patrolIndex--;
                    if (patrolIndex <= 0)
                    {
                        patrolIndex = 1;
                        isGoingForward = true;
                    }
                }
            }
            else
            {
                patrolIndex = (patrolIndex + 1) % len;
            }

            Debug.Log(patrolIndex);
        }

        private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float apexHeight)
        {
            float g = Physics.gravity.y;
            float vUp = Mathf.Sqrt(-2f * g * apexHeight);
            float tUp = vUp / -g;
            float ¥äH = apexHeight - (end.y - start.y);
            float tDown = Mathf.Sqrt(2f * ¥äH / -g);
            float totalT = tUp + tDown;
            Vector3 horiz = end - start;
            horiz.y = 0;
            Vector3 vHoriz = horiz / totalT;
            return vHoriz + Vector3.up * vUp;
        }

        private IEnumerator ResumeAfterJump(Vector3 resumePos)
        {
            yield return new WaitForSeconds(2f);
            enemyRigid.velocity = Vector3.zero;
            agent.enabled = true;
            agent.updatePosition = true;
            agent.isStopped = false;
            stop = false;
            agent.SetDestination(resumePos);
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
        private float waitTime = 2f;
        private float curTime = 0f;

        public IdleState(EnemyFSMBase enemy) : base(enemy) { }

        public override void OperateEnter()
        {
            enemy.anime.Play("Idle");
            curTime = 0f;
        }

        public override void OperateUpdate()
        {
            enemy.anime.Play("Idle");
        }

        public override void OperateExit()
        {
            curTime = 0f;
        }

        public override void OperateFixedUpdate() { }
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
        enum Phase { Windup, Strike, Cooldown }
        public float windupTime = 0.5f;
        public float strikeDuration = 0.8f;
        public float cooldownTime = 0.9f;
        public float rotationSpeed = 360f;

        private Phase phase;
        private float phaseStart;
        private bool hasHit;
        private Transform transform;
        private Animator animator;
        private IMeleeAttack meleeEnemy;

        public AttackState(EnemyFSMBase enemy) : base(enemy)
        {
            transform = enemy.transform;
            animator = enemy.anime;
            meleeEnemy = enemy as IMeleeAttack;
            if (meleeEnemy == null)
                Debug.LogError($"Enemy '{enemy.name}' does not implement IMeleeAttack");
        }

        public override void OperateEnter()
        {
            phase = Phase.Windup;
            phaseStart = Time.time;
            hasHit = false;
            if (meleeEnemy != null)
                meleeEnemy.boxCollider.enabled = false;
        }

        public override void OperateUpdate()
        {
            float elapsed = Time.time - phaseStart;
            switch (phase)
            {
                case Phase.Windup:
                    RotateTowardPlayer();
                    if (elapsed >= windupTime)
                        EnterStrike();
                    break;

                case Phase.Strike:
                    if (!hasHit && meleeEnemy != null)
                    {
                        meleeEnemy.boxCollider.enabled = true;
                        hasHit = true;
                    }
                    if (elapsed >= strikeDuration && meleeEnemy != null)
                        meleeEnemy.boxCollider.enabled = false;
                    var info = animator.GetCurrentAnimatorStateInfo(0);
                    if (info.IsName("Attack") && info.normalizedTime >= 1f)
                        EnterCooldown();
                    break;

                case Phase.Cooldown:
                    if (elapsed >= cooldownTime)
                        OperateEnter();
                    break;
            }
        }

        public override void OperateExit()
        {
            if (meleeEnemy != null)
                meleeEnemy.boxCollider.enabled = false;
            animator.CrossFade("Idle", 0.05f);
        }

        public override void OperateFixedUpdate() { }

        private void RotateTowardPlayer()
        {
            Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
            Quaternion tgt = Quaternion.LookRotation(dir);
            float newY = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                tgt.eulerAngles.y,
                rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newY, transform.eulerAngles.z);
        }

        private void EnterStrike()
        {
            phase = Phase.Strike;
            phaseStart = Time.time;
            hasHit = false;
            animator.CrossFade("Attack", 0.1f);
        }

        private void EnterCooldown()
        {
            phase = Phase.Cooldown;
            phaseStart = Time.time;
            animator.CrossFade("Idle", 0.05f);
        }
    }



    public class RangeAttackState : BaseState
    {
        enum Phase { Windup, Strike, Cooldown }
        public float windupTime = 0.5f;
        public float strikeTime = 1f;
        public float cooldownTime = 0.4f;
        public float rotationSpeed = 360f;

        private Phase phase;
        private float timer;
        private bool hasFired;
        private Transform transform;
        private IFireable fireEnemy;

        public RangeAttackState(EnemyFSMBase enemy) : base(enemy)
        {
            transform = enemy.transform;
            fireEnemy = enemy as IFireable;
            if (fireEnemy == null)
                Debug.LogError($"Enemy '{enemy.name}' does not implement IFireable");
        }

        public override void OperateEnter()
        {
            phase = Phase.Windup;
            timer = windupTime;
            hasFired = false;
        }

        public override void OperateUpdate()
        {
            timer -= Time.deltaTime;
            switch (phase)
            {
                case Phase.Windup:
                    RotateTowardPlayer();
                    if (timer <= 0f)
                    {
                        phase = Phase.Strike;
                        timer = strikeTime;
                        hasFired = false;
                        enemy.anime.CrossFade("Attack", 0.1f);
                    }
                    break;

                case Phase.Strike:
                    if (!hasFired && fireEnemy != null)
                    {
                        fireEnemy.Fire();
                        hasFired = true;
                    }
                    if (timer <= 0f)
                    {
                        phase = Phase.Cooldown;
                        timer = cooldownTime;
                        enemy.anime.CrossFade("Idle", 0.05f);
                    }
                    break;

                case Phase.Cooldown:
                    if (timer <= 0f)
                    {
                        phase = Phase.Windup;
                        timer = windupTime;
                        hasFired = false;
                    }
                    break;
            }
        }

        public override void OperateExit()
        {
            enemy.anime.CrossFade("Idle", 0.05f);
            hasFired = false;
        }

        public override void OperateFixedUpdate() { }

        private void RotateTowardPlayer()
        {
            Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
            Quaternion tgt = Quaternion.LookRotation(dir);
            float newY = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                tgt.eulerAngles.y,
                rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newY, transform.eulerAngles.z);
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