using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/PatrolBehavior/Base")]
public abstract class PatrolBehaviorSO : ScriptableObject
{
    [Header("순찰 지점들 (EnemyFSMBase에서 설정하지 않아도 됨)")]
    public PatrolPoint[] patrolPoints;

    [Header("순찰 모드 설정")]
    public float dwellDelayThreshold = 0.1f;

    /// <summary>
    /// 순찰 로직을 실행
    /// </summary>
    /// <param name="enemy">행동 수행할 Enemy 인스턴스</param>
    public abstract void Patrol(EnemyFSMBase enemy);
}
