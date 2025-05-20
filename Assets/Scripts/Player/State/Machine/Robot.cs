using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : EnemyBase
{
    public override int RequiredAmpPts => 0;
    public override int RequiredPerPts => 0;
    public override int RequiredWavPts => 0;

    protected override void Awake()
    {

        base.Awake();
    }

    public override void Patrol()
    {
        throw new System.NotImplementedException();
    }
    public override void Attack()
    {
        throw new System.NotImplementedException();
    }
    

    
}
