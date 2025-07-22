using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPatrolSOBase", menuName = "SOStae/Patrol/PatrolPoints")]
public class SOPatrolPoints : EnemyPatrolSOBase
{
    private int patrolIndex;
    [SerializeField] private float patrolSpeed = 0.1f;
    private bool isWaiting;
    private float waitStartTime;
    private bool isGoingForward;

    public override void OperateEnter()
    {
        enemy.anime.Play("Move");
        
        nav.SetSpeed(patrolSpeed);
        if (enemy.patrolPoints == null || enemy.patrolPoints.Length <= 1)
        {
            if (enemy.patrolPoints != null && enemy.patrolPoints.Length == 1)
                nav.SetDestination(enemy.patrolPoints[0].point.position);
            return;
        }

        if (!nav.hasPath)
        {
            nav.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
        }
            
    }
    [SerializeField] private float rotationSpeedDegPerSec = 180f;
    public override void OperateUpdate()
    {
        float vx = rigid.velocity.x;
        if (Mathf.Abs(vx) < 0.01f) return;

        Quaternion targetRot = Quaternion.Euler(0f, vx > 0f ? 90f : 270f, 0f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeedDegPerSec * Time.deltaTime
        );

        if (enemy.patrolPoints == null || enemy.patrolPoints.Length == 0)
            return;

        if (enemy.patrolPoints.Length == 1)
        {
            if (nav.RemainingDistance() <= nav.stoppingDistance)
            {
                nav.isStopped = true;
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
                nav.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
            }
        }
        else
        {
            if (Mathf.Abs(enemy.patrolPoints[patrolIndex].point.position.x - transform.position.x) < 0.05f)
            {
                nav.ResetPath();
                nav.velocity = Vector3.zero;

                if (enemy.patrolPoints[patrolIndex].needJump && isGoingForward)
                {
                    nav.ResetPath();
                    nav.enabled = false;
                    float apexH = enemy.patrolPoints[patrolIndex].jumpPower > 0
                                  ? enemy.patrolPoints[patrolIndex].jumpPower
                                  : enemy.defaultApexHeight;
                    AdvanceIndex();
                    Vector3 nextPos = enemy.patrolPoints[patrolIndex].point.position;
                    Vector3 launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);
                    rigid.useGravity = true;
                    rigid.velocity = launch;
                    enemy.patrolPoints[patrolIndex].needJump = false;
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
                        nav.SetDestination(enemy.patrolPoints[patrolIndex].point.position);
                    }
                }
            }
        }
    }

    public override void OperateFixedUpdate()
    {

    }

    public override void OperateExit()
    {
        nav.isStopped = true;
    }

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
        float δH = apexHeight - (end.y - start.y);
        float tDown = Mathf.Sqrt(2f * δH / -g);
        float totalT = tUp + tDown;
        Vector3 horiz = end - start;
        horiz.y = 0;
        Vector3 vHoriz = horiz / totalT;
        return vHoriz + Vector3.up * vUp;
    }

    private IEnumerator ResumeAfterJump(Vector3 resumePos)
    {
        yield return new WaitForSeconds(2f);
        rigid.velocity = Vector3.zero;
        nav.enabled = true;
        
        nav.isStopped = false;
        nav.SetDestination(resumePos);
    }
}
