using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOIdle", menuName = "Enemy Logic/Idle Logic/Idle")]
public class SOIdle : EnemyIdleSOBase
{
    private float waitTime = 2f;
    private float curTime = 0f;

    public override void OperateEnter()
    {
        enemy.anime.Play("Idle");
        curTime = 0f;
    }

    public override void OperateUpdate()
    {
        curTime += Time.deltaTime;
        if (curTime >= waitTime && enemy.patrolPoints.Length > 0)
            enemy.ChangeState(State.Patrol);
    }

    public override void OperateFixedUpdate()
    {

    }

    public override void OperateExit()
    {
        curTime = 0f;
    }
}
