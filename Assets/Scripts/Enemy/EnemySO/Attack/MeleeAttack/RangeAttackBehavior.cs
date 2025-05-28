using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AttackBehavior/Range")]
public class RangeAttackBehavior : AttackBehaviorSO
{
    enum Phase { Windup, Strike, Cooldown }

    [Tooltip("공격 준비 시간")]
    public float windupTime = 0.5f;
    [Tooltip("공격 동작 시간")]
    public float strikeTime = 0.3f;
    [Tooltip("쿨다운 시간")]
    public float cooldownTime = 0.4f;
    [Tooltip("밀쳐내기용 힘")]
    public int atkPower = 10;

    [Header("회전 속도 (°/초)")]
    public float rotationSpeed = 360f;

    public GameObject bullet;

    Rigidbody rigidb;
    CapsuleCollider cap;

    Phase phase;
    float phaseStart;
    bool hasFired;
    // Enter 때 목표 회전을 미리 계산해둡니다
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
                // 현재와 목표의 Y축 각도만 구합니다.
                float currentY = transform.eulerAngles.y;
                float targetY = targetRotation.eulerAngles.y;

                // Y축 각도만 rotationSpeed 속도로 보간
                float newY = Mathf.MoveTowardsAngle(
                    currentY,
                    targetY,
                    rotationSpeed * Time.deltaTime
                );

                // X/Z는 그대로 두고 Y만 적용
                Vector3 euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, newY, euler.z);

                if (elapsed >= windupTime)
                {
                    phase = Phase.Strike;
                    phaseStart = Time.time;
                    hasFired = false;
                    enemy.anime.CrossFade("Attack", 0.1f);
                    Debug.Log(elapsed + " wait→Strike");
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

                    // 선택: 애니는 바로 Idle로
                    enemy.anime.SetTrigger("stop");
                }

                // 애니메이션이 끝나면 Cooldown
                if (elapsed >= strikeTime + cooldownTime)
                {
                    phase = Phase.Windup;
                    phaseStart = Time.time;
                    Debug.Log(elapsed + " Strike→Windup");
                }
                break;

            case Phase.Cooldown:
                // cooldownTime은 내부 논리용 타이머만 남기고, Idle 이미 페이드했으니 바로 Windup
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
        // 필요 시 뒤처리
        hasFired = false;
    }

    public override void DoFixedUpdateLogic() { }
}
