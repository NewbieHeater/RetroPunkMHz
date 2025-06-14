using EnemyRobotState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : EnemyFSMBase<Robot>, IFireable
{
    public void Fire()
    {
        Instantiate(bullet, transform.position + Vector3.up * 1f, transform.rotation);
    }
    public override int RequiredAmpPts => 0;
    public override int RequiredPerPts => 0;
    public override int RequiredWavPts => 0;

    [SerializeField] protected GameObject questionMark;
    [SerializeField] protected GameObject bullet;

    protected override void Awake()
    {
        base.Awake();

        stateMap = new Dictionary<State, BaseState<Robot>>()
        {
            { State.Patrol,      new EnemyRobotState.PatrolState<Robot>(this)      },
            { State.Chase,       new EnemyRobotState.MoveState<Robot>(this)        },
            { State.Idle,        new EnemyRobotState.IdleState<Robot>(this)        },
            { State.MeleeAttack, new EnemyRobotState.AttackState<Robot>(this)      },
            { State.RangeAttack, new EnemyRobotState.RangeAttackState<Robot>(this) },
            { State.Death,       new EnemyRobotState.DeathState<Robot>(this)       },
        };
        transitions = new List<StateTransition<Robot>>()
        {
            new StateTransition<Robot>(
                State.Patrol, State.RangeAttack,
                () => (IsPlayerInSight(rangeAttackRange)
                    || (Vector3.Distance(transform.position, player.transform.position) < findRange
                        && Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange))
            ),
            new StateTransition<Robot>(
                State.RangeAttack, State.MeleeAttack,
                () => IsPlayerInSight(meleeAttackRange)
            ),
            new StateTransition<Robot>(
                State.Patrol, State.MeleeAttack,
                () => IsPlayerInSight(meleeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange
            ),
            new StateTransition<Robot>(
                State.MeleeAttack, State.RangeAttack,
                () => Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange
            ),
            new StateTransition<Robot>(
                State.RangeAttack, State.Idle,
                () => Vector3.Distance(transform.position, player.transform.position) > rangeAttackRange
            ),
            // ANY ¡æ Death
            new StateTransition<Robot>(
                State.ANY, State.Death,
                () => currentHp <= 0
            ),
        };
        fsm = new DataStateMachine<Robot>(State.Patrol, stateMap, transitions);
    }

    protected override void Start()
    {


        base.Start();
        CurrentState = State.Patrol;
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
