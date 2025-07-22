using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPatrolSOBase", menuName = "SOStae/Patrol")]
public class EnemyPatrolSOBase : ScriptableObject
{
    protected EnemyFSMBase enemy;
    protected Rigidbody rigid;
    protected Transform transform;
    protected GameObject gameObject;
    protected RigidNavigation nav;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;
        this.rigid = enemy.rigid;
        this.nav = enemy.rigidNav;
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
