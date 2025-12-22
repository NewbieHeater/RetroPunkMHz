using UnityEngine;

public sealed class ClimberHitState : EnemyState<Climber>
{
    private float _t;

    public ClimberHitState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Hit;

    public override void Enter()
    {
        owner.surfaceWalker.RequestStun(true, owner.HitStun);

        if (owner.Anim && !string.IsNullOrEmpty(owner.HitStateName))
            owner.Anim.CrossFade(owner.HitStateName, owner.HitCrossFade, 0);

        _t = owner.HitStun;

        // 넉백 (EnemyBase의 캐시 사용)
        Vector2 hitDir = owner.LastHitDir2D;
        if (hitDir.sqrMagnitude < 1e-6f) hitDir = Vector2.right;

        float strength = (owner.LastKnockbackForce > 0f) ? owner.LastKnockbackForce : owner.KnockbackStrength;

        float vx = hitDir.x * strength;

        var rb = owner.RigidBody;
        if (rb && owner.Nav != null)
        {
            float vy = rb.velocity.y;
            owner.Nav.ApplyExternalVelocity(new Vector3(vx, vy, 0f), owner.HitStun);
        }
    }

    public override void Tick(float dt)
    {
        _t -= dt;

        if (_t <= 0f)
        {
            if (owner.IsPlayerInSight(owner.AttackRange))
            {
                fsm.Change(StateInfo.Attack);
            }
            else
            {
                // 스냅이 아니라 “가장 가까운 순찰 포인트로 이동” 시작
                //owner.Patrol?.StartReturnToNearest();
                fsm.Change(StateInfo.Move);
            }
        }
    }
}
