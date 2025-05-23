using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy/PatrolBehavior/Simple")]
public class SimplePatrolBehavior : PatrolBehaviorSO
{
    Rigidbody enemyRigid;
    CapsuleCollider enemyCap;
    public override void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        enemyRigid = enemy.rigid;
        enemyCap = enemy.cap;

        base.Initialize(gameObject, enemy);
    }
    public override void DoEnterLogic()
    {
        agent.speed = patrolSpeed;
        if (!agent.hasPath) agent.SetDestination(patrolPoints[patrolIndex].point.position);
    }
    public override void DoExitLogic()
    {

    }
    public override void DoUpdateLogic()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;


        // 대기 처리
        if (isWaiting)
        {
            if (Time.time - waitStartTime >= patrolPoints[patrolIndex].dwellTime)
            {
                isWaiting = false;
                AdvanceIndex();
                agent.SetDestination(patrolPoints[patrolIndex].point.position);
            }
        }
        else
        {
            // 도착 판정 (X축 근접 기준)
            if (!agent.pathPending &&
                Mathf.Abs(patrolPoints[patrolIndex].point.position.x - transform.position.x) < 0.05f)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;

                // 점프 로직
                if (patrolPoints[patrolIndex].needJump && isGoingForward)
                {
                    agent.enabled = false;
                    enemyCap.enabled = true;

                    AdvanceIndex();
                    var nextPos = patrolPoints[patrolIndex].point.position;

                    float apexH = patrolPoints[patrolIndex].jumpPower > 0 ? patrolPoints[patrolIndex].jumpPower : enemy.defaultApexHeight;
                    var launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);

                    enemyRigid.useGravity = true;
                    enemyRigid.velocity = launch;

                    patrolPoints[patrolIndex].needJump = false;
                    enemy.StartCoroutine(ResumeAfterJump(nextPos));
                }
                else
                {
                    // 대기 or 즉시 이동
                    if (patrolPoints[patrolIndex].dwellTime > 0f)
                    {
                        isWaiting = true;
                        waitStartTime = Time.time;
                    }
                    else
                    {
                        AdvanceIndex();
                        agent.SetDestination(patrolPoints[patrolIndex].point.position);
                    }
                }
            }
        }

        // 인덱스 Advance 중복 방지용 내부 메서드
        void AdvanceIndex()
        {
            if (enemy.getBackAvailable)
            {
                if (isGoingForward)
                {
                    patrolIndex++;
                    if (patrolIndex >= patrolPoints.Length)
                    {
                        patrolIndex = patrolPoints.Length - 2;
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
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
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
    }
    public override void DoFixedUpdateLogic()
    {

    }
}