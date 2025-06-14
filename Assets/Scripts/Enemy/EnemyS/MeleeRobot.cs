using EnemyRobotState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRobot : EnemyFSMBase<MeleeRobot>
{
    public override int RequiredAmpPts => 0;
    public override int RequiredPerPts => 0;
    public override int RequiredWavPts => 0;

    [SerializeField] protected GameObject questionMark;

    protected override void Awake()
    {
        base.Awake();

        stateMap = new Dictionary<State, BaseState<MeleeRobot>>()
        {
            { State.Patrol,      new EnemyRobotState.PatrolState<MeleeRobot>(this)      },
            { State.Chase,       new EnemyRobotState.MoveState<MeleeRobot>(this)        },
            { State.Idle,        new EnemyRobotState.IdleState<MeleeRobot>(this)        },
            { State.MeleeAttack, new EnemyRobotState.AttackState<MeleeRobot>(this) },
            { State.Death,       new EnemyRobotState.DeathState<MeleeRobot>(this)       },
        };
        transitions = new List<StateTransition<MeleeRobot>>()
        {
            new StateTransition<MeleeRobot>(
                State.Patrol, State.MeleeAttack,
                () => (IsPlayerInSight(rangeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange)
            ),
            new StateTransition<MeleeRobot>(
                State.MeleeAttack, State.Idle,
                () => Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange
            ),
            new StateTransition<MeleeRobot>(
                State.Idle, State.MeleeAttack,
                () => Vector3.Distance(transform.position, player.transform.position) < meleeAttackRange
            ),
            // ANY ¡æ Death
            new StateTransition<MeleeRobot>(
                State.ANY, State.Death,
                () => currentHp <= 0
            ),
        };

    }

    protected override void Start()
    {


        base.Start();


    }
    public override void Update()
    {
        fsm.UpdateState();
    }

    public override void FixedUpdate()
    {
        fsm.FixedUpdateState();
    }
}
