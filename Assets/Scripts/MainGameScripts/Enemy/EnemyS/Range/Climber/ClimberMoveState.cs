using UnityEngine;

public sealed class ClimberMoveState : EnemyState<Climber>
{
    public ClimberMoveState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Move;

    public override void Enter()
    {
        owner.surfaceWalker.SetStopped(false);
    }

    public override void Tick(float dt)
    {
        if (owner.IsPlayerInSight(owner.AttackRange))
        {
            owner.SetMoving(false);
            fsm.Change(StateInfo.Attack);
        }
    }

    public override void Exit()
    {
        owner.surfaceWalker.SetStopped(true);
    }
}
