using UnityEngine;

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
    [Tooltip("밀쳐내기용 힘")]
    public int atkPower = 10;

    [Header("회전 속도 (°/초)")]
    public float rotationSpeed = 360f;

    Rigidbody rigidb;
    CapsuleCollider cap;

    Phase phase;
    float phaseStart;

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
        // 1) 플레이어 방향 계산
        

        // 2) Windup 시작
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
                    // 바로 Attack 애니메이션으로 페이드
                    enemy.anime.CrossFade("Attack", 0.1f);
                }
                break;

            case Phase.Strike:
                // strikeTime 대신, 애니 재생 시작 후 바로 데미지
                if (elapsed >= 0f && elapsed < Time.deltaTime)
                {
                    enemy.player.ModifyHp(atkPower);
                    Debug.Log("dagage");
                }

                // 애니메이션 상태를 직접 체크해서 끝나면 Cooldown 진입
                AnimatorStateInfo info = enemy.anime.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Attack") && info.normalizedTime >= 1f)
                {
                    phase = Phase.Cooldown;
                    phaseStart = Time.time;
                    // Optional: 짧은 페이드로 Idle 준비 애니로 전환
                    enemy.anime.CrossFade("Idle", 0.05f);
                }
                break;

            case Phase.Cooldown:
                // cooldownTime은 내부 논리용 타이머만 남기고, Idle 이미 페이드했으니 바로 Windup
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
        // 필요 시 뒤처리
    }

    public override void DoFixedUpdateLogic() { }
}
