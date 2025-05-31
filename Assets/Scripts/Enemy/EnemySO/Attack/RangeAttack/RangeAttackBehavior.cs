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

    [Header("회전 속도 (°/초)")]
    public float rotationSpeed = 360f;

    [Header("발사체")]
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
        // 첫 진입 시 Windup 상태로, 타이머 설정
        phase = Phase.Windup;
        timer = windupTime;
        hasFired = false;
    }

    public override void DoUpdateLogic()
    {
        // 매 프레임 타이머 감소
        timer -= Time.deltaTime;

        switch (phase)
        {
            case Phase.Windup:
                // 1) 플레이어 방향으로 Y축만 회전
                Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(dir);
                float currentY = transform.eulerAngles.y;
                float targetY = targetRotation.eulerAngles.y;
                float newY = Mathf.MoveTowardsAngle(currentY, targetY, rotationSpeed * Time.deltaTime);
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, newY, euler.z);

                // 2) 준비 시간 경과 시 Strike로 전환
                if (timer <= 0f)
                {
                    phase = Phase.Strike;
                    timer = strikeTime;
                    hasFired = false;
                    // 공격 애니메이션 재생
                    enemy.anime.CrossFade("Attack", 0.1f);
                }
                break;

            case Phase.Strike:
                // 1) 아직 발사하지 않았다면 즉시 발사
                if (!hasFired)
                {
                    Instantiate(bullet, transform.position + Vector3.up * 1f, transform.rotation);
                    hasFired = true;
                }
                // 2) strikeTime 경과 시 Cooldown으로 전환
                if (timer <= 0f)
                {
                    phase = Phase.Cooldown;
                    timer = cooldownTime;
                    enemy.anime.CrossFade("Idle", 0.05f);
                }
                break;

            case Phase.Cooldown:
                // 쿨타임 끝나면 다시 Windup
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
        // 이 공격 행동은 물리 업데이트가 필요 없으므로 비워둡니다.
    }
}
