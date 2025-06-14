using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyRobotState
{
    public class PatrolState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        protected Transform transform;
        protected Transform playerTransform;
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
            playerTransform = monster.player.transform;

            // 인덱스 초기값 설정
            patrolIndex = 0;
            isWaiting = false;
            isGoingForward = true;
        }

        public override void OperateEnter() 
        {
            Debug.Log("Enter");
            enemy.anime.Play("Move");
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            
            if (!agent.hasPath) agent.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
        }
        public override void OperateExit() { agent.enabled = false; }
        public override void OperateUpdate()
        {
            if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0) return;


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
                if (!agent.pathPending &&
                    Mathf.Abs(enemy.patrolPoints[patrolIndex].point.position.x - transform.position.x) < 0.05f)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;

                    // 점프 로직
                    if (enemy.patrolPoints[patrolIndex].needJump && isGoingForward)
                    {
                        agent.enabled = false;
                        enemyCap.enabled = true;

                        AdvanceIndex();
                        var nextPos = enemy.patrolPoints[patrolIndex].point.position;

                        float apexH = enemy.patrolPoints[patrolIndex].jumpPower > 0 ? enemy.patrolPoints[patrolIndex].jumpPower : enemy.defaultApexHeight;
                        var launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);

                        enemyRigid.useGravity = true;
                        enemyRigid.velocity = launch;

                        enemy.patrolPoints[patrolIndex].needJump = false;
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
                            Debug.Log(enemy.patrolPoints[patrolIndex].point.position);
                        }
                    }
                }
            }
        }
        void AdvanceIndex()
        {
            if (enemy.getBackAvailable)
            {
                if (isGoingForward)
                {
                    patrolIndex++;
                    if (patrolIndex >= enemy.patrolPoints.Length)
                    {
                        patrolIndex = enemy.patrolPoints.Length - 2;
                        isGoingForward = false;
                    }
                }
                else
                {
                    patrolIndex--;
                    if (patrolIndex < 0)
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

        // 점프 후 복귀
        IEnumerator ResumeAfterJump(Vector3 resumePos)
        {
            yield return new WaitForSeconds(2f);
            enemyRigid.velocity = Vector3.zero;
            enemyCap.enabled = false;
            agent.enabled = true;
            agent.updatePosition = true;
            agent.isStopped = false;
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
            //enemy.questionMark.SetActive(true);
            Debug.Log("IOdle");
            enemy.anime.Play("Idle");
            curTime = 0;
        }

        public override void OperateUpdate()
        {
            curTime += Time.deltaTime;
            if (curTime >= waitTime)
            {
                enemy.ChangeState(State.Patrol);
            }
        }

        public override void OperateExit()
        {
            curTime = 0;
        }

        public override void OperateFixedUpdate() { }
    }

    public class AttackState<TSelf> : BaseState<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
    {
        enum Phase { Windup, Strike, Cooldown }

        [Tooltip("공격 준비 시간")]
        public float windupTime = 0.5f;
        [Tooltip("공격 동작 시간")]
        public float strikeTime = 0.3f;
        [Tooltip("쿨다운 시간")]
        public float cooldownTime = 0.4f;
        [Tooltip("밀쳐내기용 힘")]
        public int atkPower = 10;

        [Header("회전 속도 (°/초)")]
        public float rotationSpeed = 360f;

        Rigidbody rigidb;
        CapsuleCollider cap;

        Phase phase;
        float phaseStart;

        protected Transform transform;
        // Enter 때 목표 회전을 미리 계산해둡니다
        private Quaternion targetRotation;
        public AttackState(TSelf monster) : base(monster) 
        { 
            transform = enemy.transform;
        }

        public override void OperateEnter()
        {
            phase = Phase.Windup;
            phaseStart = Time.time;
        }

        public override void OperateUpdate()
        {
            float elapsed = Time.time - phaseStart;

            switch (phase)
            {
                case Phase.Windup:
                    Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
                    targetRotation = Quaternion.LookRotation(dir);
                    // 현재와 목표의 Y축 각도만 구합니다.
                    float currentY = transform.eulerAngles.y;
                    float targetY = targetRotation.eulerAngles.y;

                    // Y축 각도만 rotationSpeed 속도로 보간
                    float newY = Mathf.MoveTowardsAngle(
                        currentY,
                        targetY,
                        rotationSpeed * Time.deltaTime
                    );

                    // X/Z는 그대로 두고 Y만 적용
                    Vector3 euler = transform.eulerAngles;
                    transform.eulerAngles = new Vector3(euler.x, newY, euler.z);

                    if (elapsed >= windupTime)
                    {
                        phase = Phase.Strike;
                        phaseStart = Time.time;
                        // 바로 Attack 애니메이션으로 페이드
                        enemy.anime.CrossFade("Attack", 0.1f);
                    }
                    break;

                case Phase.Strike:
                    // strikeTime 대신, 애니 재생 시작 후 바로 데미지
                    if (elapsed >= 0f && elapsed < Time.deltaTime)
                    {
                        enemy.player.ModifyHp(atkPower);
                        Debug.Log("dagage");
                    }

                    // 애니메이션 상태를 직접 체크해서 끝나면 Cooldown 진입
                    AnimatorStateInfo info = enemy.anime.GetCurrentAnimatorStateInfo(0);
                    if (info.IsName("Attack") && info.normalizedTime >= 1f)
                    {
                        phase = Phase.Cooldown;
                        phaseStart = Time.time;
                        // Optional: 짧은 페이드로 Idle 준비 애니로 전환
                        enemy.anime.CrossFade("Idle", 0.05f);
                    }
                    break;

                case Phase.Cooldown:
                    // cooldownTime은 내부 논리용 타이머만 남기고, Idle 이미 페이드했으니 바로 Windup
                    if (elapsed >= cooldownTime)
                    {
                        phase = Phase.Windup;
                        phaseStart = Time.time;
                        // attackFinishedCallback?.Invoke();
                    }
                    break;
            }
        }

        public override void OperateExit()
        {
            enemy.anime.CrossFade("Idle", 0.05f);
        }

        public override void OperateFixedUpdate() { }
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