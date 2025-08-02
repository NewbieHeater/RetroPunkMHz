using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climber : EnemyFSMBase
{
    public override int RequiredAmpPts => 0;
    public override int RequiredPerPts => 0;
    public override int RequiredWavPts => 0;

    [SerializeField] protected GameObject mQuestionMark;

    protected override void Awake()
    {
        base.Awake();

        stateMap = new Dictionary<State, BaseState>()
        {
            { State.Patrol,      new EnemyRobotState.PatrolState(this)      },
            { State.Chase,       new EnemyRobotState.MoveState(this)        },
            { State.Idle,        new EnemyRobotState.IdleState(this)        },
            { State.RangeAttack, new EnemyRobotState.RangeAttackState(this) },
            { State.Death,       new EnemyRobotState.DeathState(this)       },
        };
        transitions = new List<StateTransition>()
        {
            new StateTransition(
                State.Patrol, State.RangeAttack,
                () => (IsPlayerInSight(rangeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange)
            ),
            new StateTransition(
                State.RangeAttack, State.Idle,
                () => Vector3.Distance(transform.position, player.transform.position) > rangeAttackRange
            ),
            // ANY ¡æ Death
            new StateTransition(
                State.ANY, State.Death,
                () => mCurrentHp <= 0
            ),
        };

    }

    protected override void Start()
    {


        base.Start();

        fsm = new DataStateMachine(State.Patrol, stateMap, transitions);
        CurrentState = State.Patrol;
    }


    public override void SetQuestionMark(bool active)
    {
        mQuestionMark.SetActive(active);
    }
}
