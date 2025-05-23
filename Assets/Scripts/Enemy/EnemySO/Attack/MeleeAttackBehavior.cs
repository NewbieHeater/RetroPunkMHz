using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public float attackForce = 10f;

    Rigidbody rigidb;
    CapsuleCollider cap;
    Transform tf;
    Phase phase;
    float phaseStart;

    public override void Initialize(GameObject go, EnemyFSMBase e)
    {
        base.Initialize(go, e);
        rigidb = e.rigid;
        cap = e.cap;
        tf = go.transform;
    }

    public override void DoEnterLogic()
    {
        // 1) �÷��̾� ���� �ٶ󺸱�
        Vector3 dir = (enemy.player.position - tf.position).normalized;
        tf.forward = dir;

        // 2) Wind-up ����
        phase = Phase.Windup;
        phaseStart = Time.time;
    }

    public override void DoUpdateLogic()
    {
        float elapsed = Time.time - phaseStart;

        switch (phase)
        {
            case Phase.Windup:
                if (elapsed >= windupTime)
                {
                    // �غ� �� �� ����
                    phase = Phase.Strike;
                    phaseStart = Time.time;

                    // (���Ѵٸ�) �ݶ��̴� Ȱ��ȭ, ������ ���� ���� Ʈ����
                    //AttackHit();
                }
                break;

            case Phase.Strike:
                if (elapsed >= strikeTime)
                {
                    // ���� �Ĺݺ�(���ĳ���) ó��
                    rigidb.AddForce(tf.forward * attackForce, ForceMode.Impulse);

                    phase = Phase.Cooldown;
                    phaseStart = Time.time;
                }
                break;

            case Phase.Cooldown:
                if (elapsed >= cooldownTime)
                {
                    // �� �� ���� FSM�� ������ ���� ��ȣ
                    //attackFinishedCallback?.Invoke();
                }
                break;
        }
    }

    public override void DoExitLogic()
    {
        // �ʿ��ϸ� �ݶ��̴� ��Ȱ��ȭ �� ����
        //CleanupAfterAttack();
    }

    public override void DoFixedUpdateLogic() { }
}
