using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOPatrolPoints", menuName = "Enemy Logic/Patrol Logic/PointPatrol")]
public class SOPatrolPoints : EnemyPatrolSOBase
{
    private int patrolIndex = 0;
    [SerializeField] private float patrolSpeed = 0.1f;
    [SerializeField] private float rotationSpeedDegPerSec = 180f;
    private bool isWaiting;
    private float waitStartTime;
    [SerializeField] private bool isGoingForward;
    private Vector3[] Pos;
    private Vector3 firstPos = Vector3.zero;
    private Animator animator;

    public override void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        base.Initialize(gameObject, enemy);
        animator = enemy.anime;
        patrolIndex = 0;
        isGoingForward = true;

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

        
        isWaiting = false;
        nav.SetDestinationWalk(Pos[patrolIndex]);
        
    }
    
    public override void OperateUpdate()
    {
        

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
                nav.SetDestinationWalk(Pos[patrolIndex]);
            }
        }
        else
        {
            if (Mathf.Abs(Pos[patrolIndex].x - transform.position.x) < 0.05f)
            {
                nav.ResetPath();

                if (enemy.patrolPoints[patrolIndex].needJump && isGoingForward)
                {
                    //nav.ResetPath();
                    //float apexH = enemy.patrolPoints[patrolIndex].jumpPower > 0
                    //              ? enemy.patrolPoints[patrolIndex].jumpPower
                    //              : enemy.defaultApexHeight;
                    AdvanceIndex();
                    Vector3 nextPos = Pos[patrolIndex];
                    //Vector3 launch = CalculateLaunchVelocity(transform.position, nextPos, apexH);
                    //rigid.useGravity = true;
                    //rigid.velocity = launch;
                    
                    nav.SetDestinationJump(nextPos);
                    
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
                        nav.SetDestinationWalk(Pos[patrolIndex]);
                    }
                }
            }
        }



        Quaternion targetRot = Quaternion.Euler(0f, Pos[patrolIndex].x - transform.position.x > 0f ? 90f : 270f, 0f);

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
        Debug.Log(patrolIndex);
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
                if (patrolIndex <= -1)
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
    }
}
