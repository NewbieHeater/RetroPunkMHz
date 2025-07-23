using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleSOBase : ScriptableObject
{
    protected EnemyFSMBase enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;
        playerTransform = GameManager.Instance.player.transform;
    }

    public virtual void OperateEnter() 
    {
        
    }

    public virtual void OperateUpdate() 
    {
        
    }

    public virtual void OperateFixedUpdate() 
    {

    }

    public virtual void OperateExit()
    { 

    }
    
}
