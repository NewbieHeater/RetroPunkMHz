using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/PatrolBehavior/Simple")]
public class SimplePatrolBehavior : PatrolBehaviorSO
{
    public override void Patrol(EnemyFSMBase enemy)
    {
        // PatrolBehaviorSO 안에 정의된 patrolPoints를 사용
        var points = patrolPoints;
        int idx = enemy.patrolIndex;
        var pt = points[idx];

        // 네비메시 이동
        if (!enemy.agent.hasPath || enemy.agent.pathPending)
            enemy.agent.SetDestination(pt.point.position);

        // 도착 판정
        if (Vector3.Distance(enemy.transform.position, pt.point.position) < 0.1f)
        {
            if (pt.dwellTime > dwellDelayThreshold)
                enemy.StartCoroutine(enemy.WaitAndAdvance(pt.dwellTime));
            else
                enemy.AdvancePatrolIndex(points.Length);
        }
    }
}