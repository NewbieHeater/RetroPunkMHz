using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("���� ����")]
    public float attackRange = 1f;
    [Tooltip("���� ��ٿ�")]
    public float attackCooldown = 0.5f;
    [Tooltip("��������")]
    public int damage = 10;
    [Tooltip("�� ���̾� ����ũ")]
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

        // ���� �� ��� ���� ������ ����
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
