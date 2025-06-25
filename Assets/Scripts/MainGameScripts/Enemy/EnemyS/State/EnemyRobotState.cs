using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;

namespace EnemyRobotState
{
    public class PatrolState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        protected Transform transform;
        protected NavMeshAgent agent;
        protected GameObject gameObject;
        Rigidbody enemyRigid;
        CapsuleCollider enemyCap;
        public float patrolSpeed = 2f;
        [HideInInspector] public int patrolIndex;
        [HideInInspector] public bool isWaiting;
        [HideInInspector] public float waitStartTime;
        [HideInInspector] public bool isGoingForward;
        public PatrolState(TSelf monster) : base(monster)
        {
            transform = monster.transform;
            agent = monster.agent;
            enemyRigid = monster.rigid;
            enemyCap = monster.cap;

            // 인덱스 초기값 설정
            patrolIndex = 0;
            isWaiting = false;
            isGoingForward = true;
        }

        public override void OperateEnter()
        {
            enemy.anime.Play("Move");
            //agent.enabled = true;
            agent.isStopped = false;
            agent.speed = patrolSpeed;

            // 포인트가 1개 이하라면 바로 이동 후 멈춤 플래그 준비
            if (enemy.patrolPoints == null || enemy.patrolPoints.Length <= 1)
            {
                if (enemy.patrolPoints != null && enemy.patrolPoints.Length == 1)
                    agent.SetDestination(enemy.patrolPoints[0].point.position);

                return;
            }

            // 기존 순찰 시작 로직
            if (!agent.hasPath)
                agent.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
        }
        public override void OperateExit() { agent.isStopped = true; }

        public override void OperateUpdate()
        {
            // 포인트가 0개면 아무 것도 안 함
            if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0 || stop == true)
                return;

            // 포인트가 1개일 때: 목적지에 도착하면 멈춤 플래그 셋
            if (enemy.patrolPoints.Length == 1)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    // 멈춤 상태로 전환
                    enemy.stop = true;
                    agent.isStopped = true;
                    enemy.ChangeState(State.Idle);
                }
                return;
            }


            // 대기 처리
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
                // 도착 판정 (X축 근접 기준)
                if (
                    Mathf.Abs(enemy.patrolPoints[patrolIndex].point.position.x - transform.position.x) < 0.05f)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    // 점프 로직
                    if (enemy.patrolPoints[patrolIndex].needJump && isGoingForward)
                    {
                        Debug.Log("Jum[p");
                        agent.ResetPath();
                        agent.enabled = false;
                        
                        enemyCap.enabled = true;
                        float apexH = enemy.patrolPoints[patrolIndex].jumpPower > 0 ? enemy.patrolPoints[patrolIndex].jumpPower : enemy.defaultApexHeight;
                        AdvanceIndex();
                        var nextPos = enemy.patrolPoints[patrolIndex].point.position;

                        
                        var launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);

                        enemyRigid.useGravity = true;
                        enemyRigid.velocity = launch;
                        //enemyRigid.AddForce(launch, ForceMode.Impulse);
                        enemy.patrolPoints[patrolIndex].needJump = false;
                        stop = true;
                        enemy.StartCoroutine(ResumeAfterJump(nextPos));
                    }
                    else
                    {
                        // 대기 or 즉시 이동
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
        void AdvanceIndex()
        {
            int len = enemy.patrolPoints.Length;
            if (len <= 1)
            {
                // 포인트가 0개이거나 1개 뿐이라면 순찰 로직을 수행하지 않음
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
                        patrolIndex = enemy.patrolPoints.Length - 2;
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
                patrolIndex = (patrolIndex + 1) % enemy.patrolPoints.Length;
            }
            Debug.Log(patrolIndex);
        }

        // 발사 벨로시티 계산
        Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float apexHeight)
        {
            float g = Physics.gravity.y;
            float vUp = Mathf.Sqrt(-2f * g * apexHeight);
            float tUp = vUp / -g;
            float deltaH = apexHeight - (end.y - start.y);
            float tDown = Mathf.Sqrt(2f * deltaH / -g);
            float totalT = tUp + tDown;
            Vector3 horiz = end - start;
            horiz.y = 0;
            Vector3 vHoriz = horiz / totalT;
            return vHoriz + Vector3.up * vUp;
        }
        bool stop;
        // 점프 후 복귀
        IEnumerator ResumeAfterJump(Vector3 resumePos)
        {
            yield return new WaitForSeconds(2f);
            enemyRigid.velocity = Vector3.zero;
            //enemyCap.enabled = false;
            agent.enabled = true;
            agent.updatePosition = true;
            agent.isStopped = false;
            stop = false;
            agent.SetDestination(resumePos);
        }
        public override void OperateFixedUpdate() { }
    }

    public class MoveState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        public MoveState(TSelf monster) : base(monster) { }

        public override void OperateEnter()
        {
        }

        public override void OperateUpdate()
        {
        }

        public override void OperateExit()
        {
        }

        public override void OperateFixedUpdate() { }
    }

    public class IdleState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        private float waitTime = 2;
        private float curTime = 0;
        public IdleState(TSelf monster) : base(monster) { }

        public override void OperateEnter()
        {
            enemy.anime.Play("Idle");
            curTime = 0;
        }

        public override void OperateUpdate()
        {
            enemy.anime.Play("Idle");
        }

        public override void OperateExit()
        {
            curTime = 0;
        }

        public override void OperateFixedUpdate() { }
    }

    public class SearchState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        private float waitTime = 2;
        private float curTime = 0;
        public SearchState(TSelf monster) : base(monster) { }

        public override void OperateEnter()
        {
            enemy.SetQuestionMark(true);
            enemy.anime.Play("Idle");
            curTime = 0;
        }

        public override void OperateUpdate()
        {
            curTime += Time.deltaTime;
            if (curTime >= waitTime && enemy.patrolPoints.Length > 0)
            {

                enemy.ChangeState(State.Idle);
            }
        }

        public override void OperateExit()
        {
            enemy.SetQuestionMark(false);
            curTime = 0;
        }

        public override void OperateFixedUpdate() { }
    }

    public class AttackState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>, IMeleeAttack
    {
        enum Phase { Windup, Strike, Cooldown }

        [Header("타이밍 (초)")]
        public float windupTime = 0.5f;   // 준비 동작 시간
        public float strikeDuration = 0.8f;   // 타격 판정 유지 시간
        public float cooldownTime = 0.9f;   // 쿨다운 시간

        [Tooltip("넉백 힘")]
        public int atkPower = 10;

        [Header("회전 속도 (°/초)")]
        public float rotationSpeed = 360f;

        private Phase phase;
        private float phaseStart;
        private bool hasHit;                // ← 한 번 히트했는지 체크

        // 캐싱
        private Transform transform;
        private Animator animator;

        public AttackState(TSelf monster) : base(monster)
        {
            this.transform = enemy.transform;
            this.animator = enemy.anime;
        }

        public override void OperateEnter()
        {
            // Windup 시작 시 초기화
            phase = Phase.Windup;
            phaseStart = Time.time;
            hasHit = false;
            enemy.boxCollider.enabled = false;
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
                    // 한 번만 판정
                    if (!hasHit)
                    {
                        enemy.boxCollider.enabled = true;
                        Debug.Log("damage!");
                        hasHit = true;
                    }

                    // strikeDuration 이후 콜라이더 비활성화
                    if (elapsed >= strikeDuration)
                        enemy.boxCollider.enabled = false;

                    // 애니메이션 완료 체크
                    var info = animator.GetCurrentAnimatorStateInfo(0);
                    if (info.IsName("Attack") && info.normalizedTime >= 1f)
                        EnterCooldown();
                    break;

                case Phase.Cooldown:
                    if (elapsed >= cooldownTime)
                        OperateEnter();  // 다시 Windup 시작
                    break;
            }
        }

        public override void OperateExit()
        {
            enemy.boxCollider.enabled = false;
            animator.CrossFade("Idle", 0.05f);
        }

        public override void OperateFixedUpdate() { }

        // 플레이어 방향으로 부드럽게 회전
        private void RotateTowardPlayer()
        {
            Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
            Quaternion tgt = Quaternion.LookRotation(dir);

            float currentY = transform.eulerAngles.y;
            float targetY = tgt.eulerAngles.y;
            float newY = Mathf.MoveTowardsAngle(
                                 currentY,
                                 targetY,
                                 rotationSpeed * Time.deltaTime);

            var e = transform.eulerAngles;
            transform.eulerAngles = new Vector3(e.x, newY, e.z);
        }

        // Strike 단계 진입
        private void EnterStrike()
        {
            phase = Phase.Strike;
            phaseStart = Time.time;
            hasHit = false;
            animator.CrossFade("Attack", 0.1f);
        }

        // Cooldown 단계 진입
        private void EnterCooldown()
        {
            phase = Phase.Cooldown;
            phaseStart = Time.time;
            animator.CrossFade("Idle", 0.05f);
        }
    }


    public class RangeAttackState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>, IFireable
    {
        protected Transform transform;
        enum Phase { Windup, Strike, Cooldown }

        [Tooltip("공격 준비 시간")]
        public float windupTime = 0.5f;
        [Tooltip("공격 동작 시간")]
        public float strikeTime = 1f;
        [Tooltip("쿨다운 시간")]
        public float cooldownTime = 0.4f;

        [Header("회전 속도 (°/초)")]
        public float rotationSpeed = 360f;

        [Header("발사체")]
        public GameObject bullet;

        Rigidbody rigidb;
        CapsuleCollider cap;

        Phase phase;
        float timer;
        bool hasFired;
        Quaternion targetRotation;
        public RangeAttackState(TSelf monster) : base(monster) 
        { 
            transform = monster.transform;
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
                    Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
                    targetRotation = Quaternion.LookRotation(dir);
                    float currentY = transform.eulerAngles.y;
                    float targetY = targetRotation.eulerAngles.y;
                    float newY = Mathf.MoveTowardsAngle(currentY, targetY, rotationSpeed * Time.deltaTime);
                    var euler = transform.eulerAngles;
                    transform.eulerAngles = new Vector3(euler.x, newY, euler.z);

                    // 2) 준비 시간 경과 시 Strike로 전환
                    if (timer <= 0f)
                    {
                        phase = Phase.Strike;
                        timer = strikeTime;
                        hasFired = false;
                        // 공격 애니메이션 재생
                        enemy.anime.CrossFade("Attack", 0.1f);
                    }
                    break;

                case Phase.Strike:
                    // 1) 아직 발사하지 않았다면 즉시 발사
                    if (!hasFired)
                    {
                        enemy.Fire();
                        hasFired = true;
                    }
                    // 2) strikeTime 경과 시 Cooldown으로 전환
                    if (timer <= 0f)
                    {
                        phase = Phase.Cooldown;
                        timer = cooldownTime;
                        enemy.anime.CrossFade("Idle", 0.05f);
                    }
                    break;

                case Phase.Cooldown:
                    // 쿨타임 끝나면 다시 Windup
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
    }

    public class DeathState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        public DeathState(TSelf monster) : base(monster) { }

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