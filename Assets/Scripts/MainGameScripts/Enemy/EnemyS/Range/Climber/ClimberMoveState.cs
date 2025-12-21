using UnityEngine;

public sealed class ClimberMoveState : EnemyState<Climber>
{
    public ClimberMoveState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Move;

    public override void Enter()
    {
        owner.SetMoving(true);
        owner.ToggleMelee(false);

        if (owner.Anim && !string.IsNullOrEmpty(owner.MoveStateName))
            owner.Anim.CrossFade(owner.MoveStateName, owner.MoveCrossFade, 0);

        // 원본: BeginPatrol(RigidNavigation.MoveMode.Climb);
        owner.Patrol?.BeginPatrol(RigidNavigation.MoveMode.Climb);
    }

    public override void Tick(float dt)
    {
        // 원본: PatrolTick(RigidNavigation.MoveMode.Climb);
        owner.Patrol?.Tick(RigidNavigation.MoveMode.Climb);

        // 원본: if (IsPlayerInSight(_attackRange)) { _nav.isStopped = true; SetState(Attack); }
        if (owner.IsPlayerInSight(owner.AttackRange))
        {
            owner.SetMoving(false); // Nav.isStopped = true 포함
            fsm.Change(StateInfo.Attack);
        }
    }

    public override void Exit()
    {
        owner.SetMoving(false);

        // RobotMoveState처럼 path 정리도 같이
        owner.Nav?.ResetPath();
    }
}
