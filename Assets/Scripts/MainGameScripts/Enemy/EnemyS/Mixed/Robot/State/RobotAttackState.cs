public sealed class RobotAttackState : EnemyState<EnemyRobot>
{
    public RobotAttackState(EnemyRobot owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Attack;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);
    }

    public override void Tick(float dt)
    {
        var player = owner.Player;

        // 플레이어가 없으면 Idle로
        if (!player)
        {
            fsm.Change(StateInfo.Idle);
            return;
        }

        // 시야를 잃었고, 현재 공격 애니메이션/전이 중이 아니라면 Move로
        bool canSeePlayer = owner.Sight &&
                            owner.Sight.IsTargetInSight(player.transform, owner.AggroRange);

        if (!canSeePlayer && !owner.IsInOrTransitionToAttack())
        {
            fsm.Change(StateInfo.Move);
            return;
        }

        // 이미 공격 동작 중이면 새로 발동하지 않음
        if (owner.IsInOrTransitionToAttack())
            return;

        // 공격 발동
        owner.ToggleMelee(true);
        if (owner.Anim)
        {
            owner.Anim.ResetTrigger(owner.AttackTrigger);
            owner.Anim.SetTrigger(owner.AttackTrigger);
        }
    }

    public override void Exit()
    {
        owner.ToggleMelee(false);
    }
}
