using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/PatrolBehavior/Base")]
public abstract class PatrolBehaviorSO : ScriptableObject
{
    [Header("���� ������ (EnemyFSMBase���� �������� �ʾƵ� ��)")]
    public PatrolPoint[] patrolPoints;

    [Header("���� ��� ����")]
    public float dwellDelayThreshold = 0.1f;

    /// <summary>
    /// ���� ������ ����
    /// </summary>
    /// <param name="enemy">�ൿ ������ Enemy �ν��Ͻ�</param>
    public abstract void Patrol(EnemyFSMBase enemy);
}
