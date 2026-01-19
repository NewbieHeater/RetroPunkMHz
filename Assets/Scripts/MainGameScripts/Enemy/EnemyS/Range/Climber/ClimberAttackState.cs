using UnityEngine;

public sealed class ClimberAttackState : EnemyState<Climber>
{
    private enum APhase { Windup, Strike, Cooldown }

    private float _t;
    private bool _strikeOpened;
    private APhase _phase;

    public ClimberAttackState(Climber owner, EnemyStateMachine fsm)
        : base(owner, fsm) { }

    public override StateInfo Kind => StateInfo.Attack;

    public override void Enter()
    {
        owner.SetMoving(false);
        owner.ToggleMelee(false);

        StartAttackCycle(); // 원본 OnEnterState(Attack) 동일 역할
    }

    public override void Tick(float dt)
    {
        _t -= dt;

        switch (_phase)
        {
            case APhase.Windup:
                owner.FacePlayerY();
                if (_t <= 0f)
                    BeginPhase(APhase.Strike, owner.Strike);
                break;

            case APhase.Strike:
                if (!_strikeOpened)
                {
                    owner.ToggleMelee(true);
                    _strikeOpened = true;
                }

                if (_t <= 0f)
                {
                    owner.ToggleMelee(false);
                    BeginPhase(APhase.Cooldown, owner.Cooldown);
                }
                break;

            case APhase.Cooldown:
                if (_t <= 0f)
                {
                    // 원본 분기:
                    // if (IsPlayerInSight(_attackRange)) SetState(Attack);
                    // else if (IsPlayerInSight(_aggroRange)) SetState(Move);
                    // else SetState(Idle);

                    if (owner.IsPlayerInSight(owner.AttackRange))
                    {
                        // 같은 Attack 상태로 재진입하지 않고, 내부 사이클을 다시 시작 (원본 기능 보존 + FSM 구현차 안전)
                        StartAttackCycle();
                        return;
                    }

                    if (owner.IsPlayerInSight(owner.AggroRange))
                        fsm.Change(StateInfo.Move);
                    else
                        fsm.Change(StateInfo.Idle);
                }
                break;
        }
    }

    public override void Exit()
    {
        // 원본 OnExitState(Attack): ToggleMelee(false)
        owner.ToggleMelee(false);
    }

    private void StartAttackCycle()
    {
        if (owner.Anim && !string.IsNullOrEmpty(owner.AttackStateName))
            owner.Anim.CrossFade(owner.AttackStateName, owner.AttackCrossFade, 0);

        owner.ToggleMelee(false);
        BeginPhase(APhase.Windup, owner.Windup);
    }

    private void BeginPhase(APhase p, float dur)
    {
        _phase = p;
        _t = Mathf.Max(0f, dur);

        // 원본: if (p != Strike) _strikeOpened = false;
        if (p != APhase.Strike)
            _strikeOpened = false;
    }
}
