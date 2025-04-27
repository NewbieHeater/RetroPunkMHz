using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("공격 범위")]
    public float attackRange = 1f;
    [Tooltip("공격 쿨다운")]
    public float attackCooldown = 0.5f;
    [Tooltip("데미지량")]
    public int damage = 10;
    [Tooltip("적 레이어 마스크")]
    public LayerMask enemyLayer;

    bool canAttack = true;
    Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void ProcessInput()
    {
        if (canAttack && Input.GetMouseButtonDown(0))
            TryAttack();
    }

    void TryAttack()
    {
        canAttack = false;
        //animator.SetTrigger("Attack");

        // 범위 내 모든 적에 데미지 적용
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * attackRange,
            attackRange,
            enemyLayer
        );
        foreach (var hit in hits)
        {
            hit.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack() => canAttack = true;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + transform.forward * attackRange,
            attackRange
        );
    }
}
