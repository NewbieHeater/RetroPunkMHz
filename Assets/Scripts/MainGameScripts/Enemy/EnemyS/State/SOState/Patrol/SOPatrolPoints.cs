using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "SOPatrolPoints", menuName = "Enemy Logic/Patrol Logic/PointPatrol")]
public class SOPatrolPoints : EnemyPatrolSOBase
{
    private int patrolIndex = 0;
    [SerializeField] private float patrolSpeed = 0.1f;
    [SerializeField] private float rotationSpeedDegPerSec = 180f;
    private bool isWaiting;
    private float waitStartTime;
    private bool isGoingForward;
    private Vector3[] Pos;
    private Vector3 firstPos = Vector3.zero;
    private Animator animator;

    public override void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        base.Initialize(gameObject, enemy);
        animator = enemy.anime;
        Pos = new Vector3[enemy.patrolPoints.Length + 1];
        Pos[0] = transform.position + transform.TransformDirection(enemy.patrolPoints[0].relativeMovePoint);
        for (int i = 1; i < enemy.patrolPoints.Length; i++)  //0번은 이미 채웠으므로 1번부터 채운다.        
        {            
            Pos[i] = Pos[i - 1] + transform.TransformDirection(enemy.patrolPoints[i].relativeMovePoint);  
        }
    }

    public override void OperateEnter()
    {
        enemy.anime.Play("Move");

        nav.SetSpeed(patrolSpeed);
        nav.ResetPath();

        // 첫 번째 목적지
        patrolIndex = 0;
        isWaiting = false;
        nav.SetDestination(Pos[patrolIndex]);
    }
    
    public override void OperateUpdate()
    {
        Debug.Log(patrolIndex);

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
            
            Debug.Log(Time.time - waitStartTime);
            if (Time.time - waitStartTime >= enemy.patrolPoints[patrolIndex].dwellTime)
            {
                
                isWaiting = false;
                AdvanceIndex();
                nav.SetDestination(Pos[patrolIndex]);
            }
        }
        else
        {
            if (Mathf.Abs(Pos[patrolIndex].x - transform.position.x) < 0.05f)
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
                    Vector3 nextPos = Pos[patrolIndex];
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
                        Debug.Log("Stop");
                        waitStartTime = Time.time;
                    }
                    else
                    {
                        AdvanceIndex();
                        nav.SetDestination(Pos[patrolIndex]);
                    }
                }
            }
        }

        float vx = rigid.velocity.x;
        if (Mathf.Abs(vx) < 0.01f) return;

        Quaternion targetRot = Quaternion.Euler(0f, vx > 0f ? 90f : 270f, 0f);

        animator.transform.rotation = Quaternion.RotateTowards(
            animator.transform.rotation,
            targetRot,
            rotationSpeedDegPerSec * Time.deltaTime
        );
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
