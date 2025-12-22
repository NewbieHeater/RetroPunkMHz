using UnityEngine;

public sealed class RobotMoveState : EnemyState<EnemyRobot>
{
    public RobotMoveState(EnemyRobot owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Move;

    public override void Enter()
    {
        owner.SetMoving(true);
        owner.ToggleMelee(false);

        if (owner.Anim && !string.IsNullOrEmpty(owner.MoveStateName))
            owner.Anim.Play(owner.MoveStateName);

        // ¼øÂû ½ÃÀÛ
        owner.Patrol?.BeginPatrol();
    }

    public override void Tick(float dt)
    {
        // ¼øÂû ÁøÇà
        owner.Patrol?.Tick();
        var player = owner.Player;
        if (player && owner.Sight &&
            owner.Sight.IsTargetInSight(player.transform, owner.AggroRange))
        {
            fsm.Change(StateInfo.Attack);
        }
    }

    public override void Exit()
    {
        owner.SetMoving(false);
        owner.Nav?.ResetPath();
    }
}
