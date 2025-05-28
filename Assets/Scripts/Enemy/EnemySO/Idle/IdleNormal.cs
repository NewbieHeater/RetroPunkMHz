using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/IdleBehavior/IdleNormal")]
public class IdleNormal : IdleBehaviorSO
{
    private float waitTime = 2;
    private float curTime = 0;
    
    public override void DoEnterLogic()
    {
        enemy.questionMark.SetActive(true);
        Debug.Log("IOdle");
        enemy.anime.Play("Idle");
        curTime = 0;
    }

    public override void DoExitLogic()
    {
        enemy.questionMark.SetActive(false);
        curTime = 0;
    }

    public override void DoFixedUpdateLogic()
    {
        throw new System.NotImplementedException();
    }

    public override void DoUpdateLogic()
    {
        curTime += Time.deltaTime;
        if (curTime >= waitTime)
        {
            enemy.ChangeState(State.Patrol);
        }
    }
}
