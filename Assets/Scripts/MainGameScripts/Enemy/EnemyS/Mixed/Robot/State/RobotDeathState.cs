public sealed class RobotDeathState : EnemyState<EnemyRobot>
{
    public RobotDeathState(EnemyRobot owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Death;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        if (owner.Nav)
            owner.Nav.SetEnabled(false);

        if (owner.Anim)
        {
            owner.Anim.ResetTrigger(owner.DeathStateName);
            owner.Anim.SetTrigger(owner.DeathStateName);
        }
    }

    public override void Tick(float dt)
    {
        // 필요 시 사망 상태에서의 루프 로직 추가 (예: 천천히 사라짐)
    }
}
