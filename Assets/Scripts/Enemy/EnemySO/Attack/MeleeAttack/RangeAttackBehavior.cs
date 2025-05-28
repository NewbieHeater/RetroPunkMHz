using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("���ĳ���� ��")]
    public int atkPower = 10;

    [Header("ȸ�� �ӵ� (��/��)")]
    public float rotationSpeed = 360f;

    public GameObject bullet;

    Rigidbody rigidb;
    CapsuleCollider cap;

    Phase phase;
    float phaseStart;
    bool hasFired;
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
        phase = Phase.Windup;
        phaseStart = Time.time;
        hasFired = false;
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
                    hasFired = false;
                    enemy.anime.CrossFade("Attack", 0.1f);
                    Debug.Log(elapsed + " wait��Strike");
                }
                break;

            case Phase.Strike:
                if (!hasFired && elapsed >= strikeTime)
                {
                    Instantiate(bullet,
                                transform.position + Vector3.up * 1f,
                                transform.rotation);
                    Debug.Log(elapsed + " attacking");
                    hasFired = true;

                    // ����: �ִϴ� �ٷ� Idle��
                    enemy.anime.SetTrigger("stop");
                }

                // �ִϸ��̼��� ������ Cooldown
                if (elapsed >= strikeTime + cooldownTime)
                {
                    phase = Phase.Windup;
                    phaseStart = Time.time;
                    Debug.Log(elapsed + " Strike��Windup");
                }
                break;

            case Phase.Cooldown:
                // cooldownTime�� ���� ���� Ÿ�̸Ӹ� �����, Idle �̹� ���̵������� �ٷ� Windup
                if (elapsed >= cooldownTime)
                {
                    phase = Phase.Windup;
                    phaseStart = Time.time;
                    Debug.Log(elapsed + "end");
                }
                break;
        }
    }

    public override void DoExitLogic()
    {
        // �ʿ� �� ��ó��
        hasFired = false;
    }

    public override void DoFixedUpdateLogic() { }
}
