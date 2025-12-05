using UnityEngine;

public sealed class RobotHitState : EnemyState<EnemyRobot>
{
    private float _elapsed;
    private float _speedX;  // 초당 이동량 (X)

    public RobotHitState(EnemyRobot owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Hit;

    public override void Enter()
    {
        _elapsed = 0f;
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        // 필요하면 히트 애니메이션
        owner.Anim.ResetTrigger(owner.HitStateName);
        owner.Anim.SetTrigger(owner.HitStateName);

        // === EnemyBase에서 캐싱해 둔 마지막 히트 정보 사용 ===
        Vector2 hitDir = owner.LastHitDir2D;
        float force = owner.LastKnockbackForce;

        // 기본적으로 SourceDir.x 부호를 사용
        float dirX;
        if (Mathf.Abs(hitDir.x) > 1e-3f)
        {
            dirX = Mathf.Sign(hitDir.x);   // < 0 : 왼쪽, > 0 : 오른쪽
        }
        else
        {
            // X성분이 거의 없으면, 보는 방향의 반대편으로 밀기
            float facing =
                Mathf.Sign(owner.transform.localScale.x == 0 ? 1f : owner.transform.localScale.x);
            dirX = -facing;
        }

        // KnockbackForce가 0이면 기본 Strength 사용
        float strength = (force > 0f) ? force : owner.KnockbackStrength;

        // 초당 이동 속도
        _speedX = dirX * strength * 6;
    }

    public override void Tick(float dt)
    {
        _elapsed += dt;

        // Transform 기반 X축 넉백
        

        if (_elapsed <= owner.HitStun)
        {
            Vector3 pos = owner.transform.position;
            pos.x += _speedX * dt;
            owner.transform.position = pos;
        }
        if (_elapsed >= owner.HitStun * 4)
        {
            fsm.Change(StateInfo.Idle);
        }
    }
}
