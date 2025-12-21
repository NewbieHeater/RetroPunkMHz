using UnityEngine;

public sealed class ClimberHitState : EnemyState<Climber>
{
    private float _t;

    public ClimberHitState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Hit;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        // 원본: _animator.CrossFade("Hit", 0.05f, 0);
        if (owner.Anim && !string.IsNullOrEmpty(owner.HitStateName))
            owner.Anim.CrossFade(owner.HitStateName, owner.HitCrossFade, 0);

        _t = owner.HitStun;
    }

    public override void Tick(float dt)
    {
        _t -= dt;

        // ===== 원본 기능 그대로: 플레이어 반대 방향 고정 넉백 =====
        var player = owner.Player;
        if (player)
        {
            float dir = Mathf.Sign(owner.transform.position.x - player.transform.position.x);
            Vector3 kb = new Vector3(dir * owner.KnockbackStrength, 0f, 0f);

            var rb = owner.RigidBody;
            if (rb)
            {
                rb.velocity = new Vector3(kb.x, rb.velocity.y, 0f);
            }
            else
            {
                owner.transform.position += kb * dt;
            }
        }

        // 원본: if (_t <= 0f) { if (IsPlayerInSight(_attackRange)) Attack else Move; }
        if (_t <= 0f)
        {
            if (owner.IsPlayerInSight(owner.AttackRange))
                fsm.Change(StateInfo.Attack);
            else
                fsm.Change(StateInfo.Move);
        }
    }

    public override void Exit()
    {
        // nothing
    }
}
