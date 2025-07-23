using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOIdle", menuName = "Enemy Logic/Idle Logic/Idle")]
public class SOIdle : EnemyIdleSOBase
{
    public override void OperateEnter()
    {
        enemy.anime.Play("Idle");
    }

    public override void OperateUpdate()
    {
        enemy.anime.Play("Idle");
    }

    public override void OperateFixedUpdate()
    {

    }

    public override void OperateExit()
    {

    }
}
