using UnityEngine;

public sealed class ClimberDeathState : EnemyState<Climber>
{
    public ClimberDeathState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Death;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        // 원본: _animator.Play("Death", 0, 0f);
        if (owner.Anim && !string.IsNullOrEmpty(owner.DeathStateName))
            owner.Anim.Play(owner.DeathStateName, 0, 0f);
    }

    public override void Tick(float dt)
    {
        // 사망 상태에서는 보통 아무 것도 안 함
    }

    public override void Exit()
    {
        // nothing
    }
}
