using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[CreateAssetMenu(menuName = "Enemy/AttackBehavior/MeleeAttack")]
public class MeleeAttackBehavior : AttackBehaviorSO
{
    enum Phase { Windup, Strike, Cooldown }

    [Tooltip("공격 준비 시간")]
    public float windupTime = 0.5f;
    [Tooltip("공격 동작 시간")]
    public float strikeTime = 0.3f;
    [Tooltip("쿨다운 시간")]
    public float cooldownTime = 0.4f;
    

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
        // 1) 플레이어 방향 바라보기
        Vector3 dir = (enemy.player.position - tf.position).normalized;
        tf.forward = dir;

        // 2) Wind-up 시작
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
                    // 준비 끝 → 공격
                    phase = Phase.Strike;
                    phaseStart = Time.time;

                    // (원한다면) 콜라이더 활성화, 데미지 판정 로직 트리거
                    //AttackHit();
                }
                break;

            case Phase.Strike:
                if (elapsed >= strikeTime)
                {
                    // 공격 후반부(밀쳐내기) 처리
                    e.player.ModifyHp(atkPower);

                    phase = Phase.Cooldown;
                    phaseStart = Time.time;
                }
                break;

            case Phase.Cooldown:
                if (elapsed >= cooldownTime)
                {
                    // 끝 → 상위 FSM에 “공격 끝” 신호
                    //attackFinishedCallback?.Invoke();
                }
                break;
        }
    }

    public override void DoExitLogic()
    {
        // 필요하면 콜라이더 비활성화 등 정리
        //CleanupAfterAttack();
    }

    public override void DoFixedUpdateLogic() { }
}
