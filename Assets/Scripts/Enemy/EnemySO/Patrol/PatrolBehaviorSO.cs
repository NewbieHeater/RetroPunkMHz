using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Enemy/PatrolBehavior/Base")]
public abstract class PatrolBehaviorSO : ScriptableObject
{
    protected EnemyFSMBase enemy;
    protected Transform transform;
    protected Transform playerTransform;
    protected NavMeshAgent agent;
    protected GameObject gameObject;

    [Header("���� ������")]
    [Tooltip("���� �ӵ�")]
    public float patrolSpeed = 5f;

    [Tooltip("������ ���� �迭")]
    public PatrolPoint[] patrolPoints;

    [HideInInspector] public int patrolIndex;
    [HideInInspector] public bool isWaiting;
    [HideInInspector] public float waitStartTime;
    [HideInInspector] public bool isGoingForward;

    public virtual void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject     = gameObject;
        transform           = gameObject.transform;
        this.enemy          = enemy;
        playerTransform     = enemy.player.transform;
        this.patrolPoints   = enemy.patrolPoints;
        this.agent          = enemy.agent;
        Debug.Log("adsf");
        patrolIndex = 0;
        isWaiting = false;
        waitStartTime = 0f;
        isGoingForward = true;
    }

    public abstract void DoEnterLogic();
    public abstract void DoExitLogic();

    public abstract void DoUpdateLogic();
    public abstract void DoFixedUpdateLogic();
}