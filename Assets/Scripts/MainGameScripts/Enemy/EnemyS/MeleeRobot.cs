using EnemyRobotState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRobot : EnemyFSMBase, IMeleeAttack
{
    [SerializeField] public BoxCollider boxCollider { get; set; }
    public override int RequiredAmpPts => 0;
    public override int RequiredPerPts => 0;
    public override int RequiredWavPts => 0;

    [SerializeField] protected GameObject questionMark;

    protected override void Awake()
    {
        base.Awake();

        stateMap = new Dictionary<State, BaseState>()
        {
            { State.Patrol,      new EnemyRobotState.PatrolState(this)      },
            { State.Chase,       new EnemyRobotState.MoveState(this)        },
            { State.Idle,        new EnemyRobotState.IdleState(this)        },
            { State.MeleeAttack, new EnemyRobotState.AttackState(this) },
            { State.Death,       new EnemyRobotState.DeathState(this)       },
        };
        transitions = new List<StateTransition>()
        {
            new StateTransition(
                State.Patrol, State.MeleeAttack,
                () => (IsPlayerInSight(rangeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange)
            ),
            new StateTransition(
                State.MeleeAttack, State.Idle,
                () => Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange
            ),
            new StateTransition(
                State.Idle, State.MeleeAttack,
                () => Vector3.Distance(transform.position, player.transform.position) < meleeAttackRange
            ),
            // ANY ¡æ Death
            new StateTransition(
                State.ANY, State.Death,
                () => currentHp <= 0
            ),
        };

    }

    protected override void Start()
    {


        base.Start();


    }

    public override void SetQuestionMark(bool active)
    {
        questionMark.SetActive(active);
    }
}
