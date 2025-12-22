using UnityEngine;

public sealed class ClimberIdleState : EnemyState<Climber>
{
    public ClimberIdleState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Idle;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        if (owner.Anim && !string.IsNullOrEmpty(owner.IdleStateName))
            owner.Anim.Play(owner.IdleStateName, 0, 0f);
    }

    public override void Tick(float dt)
    {
        // 원본 Climber의 base.TickIdle() 컨셉:
        // "플레이어 보이면 Attack, 아니면 Move"
        if (owner.IsPlayerInSight(owner.AttackRange))
        {
            fsm.Change(StateInfo.Attack);
            return;
        }

        // 플레이어 안 보이면 순찰/이동 상태로
        fsm.Change(StateInfo.Move);
    }

    public override void Exit()
    {
        // nothing
    }
}
