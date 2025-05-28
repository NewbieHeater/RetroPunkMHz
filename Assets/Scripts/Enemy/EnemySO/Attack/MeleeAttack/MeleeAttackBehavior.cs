using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AttackBehavior/MeleeAttack")]
public class MeleeAttackBehavior : AttackBehaviorSO
{
    enum Phase { Windup, Strike, Cooldown }

    [Tooltip("���� �غ� �ð�")]
    public float windupTime = 0.5f;
    [Tooltip("���� ���� �ð�")]
    public float strikeTime = 0.3f;
    [Tooltip("��ٿ� �ð�")]
    public float cooldownTime = 0.4f;
    [Tooltip("���ĳ���� ��")]
    public int atkPower = 10;

    [Header("ȸ�� �ӵ� (��/��)")]
    public float rotationSpeed = 360f;

    Rigidbody rigidb;
    CapsuleCollider cap;

    Phase phase;
    float phaseStart;

    // Enter �� ��ǥ ȸ���� �̸� ����صӴϴ�
    private Quaternion targetRotation;

    public override void Initialize(GameObject go, EnemyFSMBase e)
    {
        base.Initialize(go, e);
        rigidb = e.rigid;
        cap = e.cap;
    }

    public override void DoEnterLogic()
    {
        // 1) �÷��̾� ���� ���
        

        // 2) Windup ����
        phase = Phase.Windup;
        phaseStart = Time.time;
    }

    public override void DoUpdateLogic()
    {
        float elapsed = Time.time - phaseStart;

        switch (phase)
        {
            case Phase.Windup:
                Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(dir);
                // ����� ��ǥ�� Y�� ������ ���մϴ�.
                float currentY = transform.eulerAngles.y;
                float targetY = targetRotation.eulerAngles.y;

                // Y�� ������ rotationSpeed �ӵ��� ����
                float newY = Mathf.MoveTowardsAngle(
                    currentY,
                    targetY,
                    rotationSpeed * Time.deltaTime
                );

                // X/Z�� �״�� �ΰ� Y�� ����
                Vector3 euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, newY, euler.z);

                if (elapsed >= windupTime)
                {
                    phase = Phase.Strike;
                    phaseStart = Time.time;
                    // �ٷ� Attack �ִϸ��̼����� ���̵�
                    enemy.anime.CrossFade("Attack", 0.1f);
                }
                break;

            case Phase.Strike:
                // strikeTime ���, �ִ� ��� ���� �� �ٷ� ������
                if (elapsed >= 0f && elapsed < Time.deltaTime)
                {
                    enemy.player.ModifyHp(atkPower);
                    Debug.Log("dagage");
                }

                // �ִϸ��̼� ���¸� ���� üũ�ؼ� ������ Cooldown ����
                AnimatorStateInfo info = enemy.anime.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Attack") && info.normalizedTime >= 1f)
                {
                    phase = Phase.Cooldown;
                    phaseStart = Time.time;
                    // Optional: ª�� ���̵�� Idle �غ� �ִϷ� ��ȯ
                    enemy.anime.CrossFade("Idle", 0.05f);
                }
                break;

            case Phase.Cooldown:
                // cooldownTime�� ���� ���� Ÿ�̸Ӹ� �����, Idle �̹� ���̵������� �ٷ� Windup
                if (elapsed >= cooldownTime)
                {
                    phase = Phase.Windup;
                    phaseStart = Time.time;
                    // attackFinishedCallback?.Invoke();
                }
                break;
        }
    }

    public override void DoExitLogic()
    {
        // �ʿ� �� ��ó��
    }

    public override void DoFixedUpdateLogic() { }
}
