using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.VFX;

public abstract class EnemyBase : MonoBehaviour
{

    public Dictionary<string, IState<EnemyBase>> dicState = new Dictionary<string, IState<EnemyBase>>();
    public StateMachine<EnemyBase> sm;


    #region 초기화 및 컴포넌트 캐싱
    protected virtual void OnEnable()
    {


        dicState.Clear();
        dicState.Add("Attack", new EnemyAttackState());
        dicState.Add("Patroll", new EnemyPatrollState());
        dicState.Add("Move", new EnemyMoveState());
        dicState.Add("Idle", new EnemyIdleState());
        sm = new StateMachine<EnemyBase>(this, dicState["Patroll"]);

    }
    #endregion

    
}
