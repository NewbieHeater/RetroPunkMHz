public sealed class RobotIdleState : EnemyState<EnemyRobot>
{
    public RobotIdleState(EnemyRobot owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Idle;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        if (owner.Anim && !string.IsNullOrEmpty(owner.IdleStateName))
            owner.Anim.Play(owner.IdleStateName);
    }

    public override void Tick(float dt)
    {
        var player = owner.Player;

        if (player && owner.Sight &&
            owner.Sight.IsTargetInSight(player.transform, owner.AggroRange))
        {
            fsm.Change(StateInfo.Attack);
        }
        else
        {
            fsm.Change(StateInfo.Move);
        }
    }
}
