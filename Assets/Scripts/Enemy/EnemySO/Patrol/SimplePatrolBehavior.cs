using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/PatrolBehavior/Simple")]
public class SimplePatrolBehavior : PatrolBehaviorSO
{
    public override void Patrol(EnemyFSMBase enemy)
    {
        // PatrolBehaviorSO �ȿ� ���ǵ� patrolPoints�� ���
        var points = patrolPoints;
        int idx = enemy.patrolIndex;
        var pt = points[idx];

        // �׺�޽� �̵�
        if (!enemy.agent.hasPath || enemy.agent.pathPending)
            enemy.agent.SetDestination(pt.point.position);

        // ���� ����
        if (Vector3.Distance(enemy.transform.position, pt.point.position) < 0.1f)
        {
            if (pt.dwellTime > dwellDelayThreshold)
                enemy.StartCoroutine(enemy.WaitAndAdvance(pt.dwellTime));
            else
                enemy.AdvancePatrolIndex(points.Length);
        }
    }
}