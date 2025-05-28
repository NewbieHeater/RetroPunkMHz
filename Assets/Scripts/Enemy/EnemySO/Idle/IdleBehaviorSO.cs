using UnityEngine;
using UnityEngine.AI;

public abstract class IdleBehaviorSO : ScriptableObject
{
    protected EnemyFSMBase enemy;
    protected Transform transform;
    protected Transform playerTransform;
    protected NavMeshAgent agent;
    protected GameObject gameObject;


    public virtual void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;
        playerTransform = enemy.player.transform;
        this.agent = enemy.agent;
    }

    public abstract void DoEnterLogic();
    public abstract void DoExitLogic();
    public abstract void DoUpdateLogic();
    public abstract void DoFixedUpdateLogic();
}