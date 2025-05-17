using UnityEngine;

public class EnemyPatrollState : IState<EnemyBase>
{
    EnemyBase enemyBase;

    public void OperateEnter(EnemyBase sender)
    {
        enemyBase = sender;

    }

    public void OperateExit(EnemyBase sender)
    {

    }

    public void OperateUpdate(EnemyBase sender)
    {

    }

    public void OperateFixedUpdate(EnemyBase sender)
    {

    }
}