using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOMixedAttack", menuName = "Enemy Logic/Attack Logic/Mixed")]
public class SOMixedAttack : EnemyAttackSOBase
{
    [Tooltip("���� SO ���� (SOMeleeAttack)")]
    public SOMeleeAttack meleeAttackSO;
    [Tooltip("���Ÿ� SO ���� (SORangeAttack)")]
    public SORangeAttack rangedAttackSO;

    private EnemyAttackSOBase currentSO;

    public override void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        base.Initialize(gameObject, enemy);
        meleeAttackSO.Initialize(gameObject, enemy);
        rangedAttackSO.Initialize(gameObject, enemy);
    }

    public override void OperateEnter()
    {
        float dist = Vector3.Distance(enemy.transform.position, playerTransform.position);
        currentSO = dist <= meleeAttackSO.range ? (EnemyAttackSOBase)meleeAttackSO : (EnemyAttackSOBase)rangedAttackSO;
        currentSO.OperateEnter();
    }

    public override void OperateUpdate()
    {
        float dist = Vector3.Distance(enemy.transform.position, playerTransform.position);
        currentSO = dist <= meleeAttackSO.range ? (EnemyAttackSOBase)meleeAttackSO : (EnemyAttackSOBase)rangedAttackSO;
        currentSO?.OperateUpdate();
    }

    public override void OperateExit()
    {
        currentSO?.OperateExit();
    }
}
