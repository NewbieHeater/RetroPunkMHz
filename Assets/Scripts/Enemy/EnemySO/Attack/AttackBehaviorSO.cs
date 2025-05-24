using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Enemy/AttackBehavior/Base")]
public abstract class AttackBehaviorSO : ScriptableObject
{
    protected EnemyFSMBase enemy;
    protected Transform transform;
    protected Transform playerTransform;
    protected NavMeshAgent agent;
    protected GameObject gameObject;

    protected int atkPower = 0;


    public virtual void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject = gameObject;
        this.transform = gameObject.transform;
        this.enemy = enemy;
        this.playerTransform = enemy.player.transform;
        this.agent = enemy.agent;
        this.atkPower = enemy.atkPower
    }

    public abstract void DoEnterLogic();
    public abstract void DoExitLogic();
    public abstract void DoUpdateLogic();
    public abstract void DoFixedUpdateLogic();
}
