using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyIdleSOBase", menuName = "SOStae/Idle")]
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
        enemy.anime.Play("Idle");
    }

    public virtual void OperateUpdate() 
    {
        enemy.anime.Play("Idle");
    }

    public virtual void OperateFixedUpdate() 
    {

    }

    public virtual void OperateExit()
    { 

    }
    
}
