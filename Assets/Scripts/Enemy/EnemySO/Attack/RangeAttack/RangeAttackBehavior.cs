using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AttackBehavior/Range")]
public class RangeAttackBehavior : AttackBehaviorSO
{
    enum Phase { Windup, Strike, Cooldown }

    [Tooltip("���� �غ� �ð�")]
    public float windupTime = 0.5f;
    [Tooltip("���� ���� �ð�")]
    public float strikeTime = 0.3f;
    [Tooltip("��ٿ� �ð�")]
    public float cooldownTime = 0.4f;

    [Header("ȸ�� �ӵ� (��/��)")]
    public float rotationSpeed = 360f;

    [Header("�߻�ü")]
    public GameObject bullet;

    Rigidbody rigidb;
    CapsuleCollider cap;

    Phase phase;
    float timer;
    bool hasFired;
    Quaternion targetRotation;

    public override void Initialize(GameObject go, EnemyFSMBase e)
    {
        base.Initialize(go, e);
        rigidb = e.rigid;
        cap = e.cap;
    }

    public override void DoEnterLogic()
    {
        // ù ���� �� Windup ���·�, Ÿ�̸� ����
        phase = Phase.Windup;
        timer = windupTime;
        hasFired = false;
    }

    public override void DoUpdateLogic()
    {
        // �� ������ Ÿ�̸� ����
        timer -= Time.deltaTime;

        switch (phase)
        {
            case Phase.Windup:
                // 1) �÷��̾� �������� Y�ุ ȸ��
                Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(dir);
                float currentY = transform.eulerAngles.y;
                float targetY = targetRotation.eulerAngles.y;
                float newY = Mathf.MoveTowardsAngle(currentY, targetY, rotationSpeed * Time.deltaTime);
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, newY, euler.z);

                // 2) �غ� �ð� ��� �� Strike�� ��ȯ
                if (timer <= 0f)
                {
                    phase = Phase.Strike;
                    timer = strikeTime;
                    hasFired = false;
                    // ���� �ִϸ��̼� ���
                    enemy.anime.CrossFade("Attack", 0.1f);
                }
                break;

            case Phase.Strike:
                // 1) ���� �߻����� �ʾҴٸ� ��� �߻�
                if (!hasFired)
                {
                    Instantiate(bullet, transform.position + Vector3.up * 1f, transform.rotation);
                    hasFired = true;
                }
                // 2) strikeTime ��� �� Cooldown���� ��ȯ
                if (timer <= 0f)
                {
                    phase = Phase.Cooldown;
                    timer = cooldownTime;
                    enemy.anime.CrossFade("Idle", 0.05f);
                }
                break;

            case Phase.Cooldown:
                // ��Ÿ�� ������ �ٽ� Windup
                if (timer <= 0f)
                {
                    phase = Phase.Windup;
                    timer = windupTime;
                    hasFired = false;
                }
                break;
        }
    }

    public override void DoExitLogic()
    {
        enemy.anime.CrossFade("Idle", 0.05f);
        hasFired = false;
    }

    public override void DoFixedUpdateLogic()
    {
        // �� ���� �ൿ�� ���� ������Ʈ�� �ʿ� �����Ƿ� ����Ӵϴ�.
    }
}
