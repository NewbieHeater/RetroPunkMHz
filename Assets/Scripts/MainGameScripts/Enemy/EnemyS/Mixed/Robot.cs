using EnemyRobotState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : EnemyFSMBase
{
    

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
            { State.Idle,        new EnemyRobotState.IdleState(this)        },
            { State.Search,      new EnemyRobotState.SearchState(this)      },
            { State.MeleeAttack, new EnemyRobotState.AttackState(this)      },
            { State.RangeAttack, new EnemyRobotState.RangeAttackState(this) },
            { State.Death,       new EnemyRobotState.DeathState(this)       },
        };
        transitions = new List<StateTransition>()
        {
            new StateTransition(
                State.Patrol, State.MeleeAttack,
                () => IsPlayerInSight(rangeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange
            ),
            new StateTransition(
                State.MeleeAttack, State.Search,
                () => (Vector3.Distance(transform.position, player.transform.position) > rangeAttackRange || !IsPlayerInSight(rangeAttackRange)) && Vector3.Distance(transform.position, player.transform.position) > findRange
            ),
            // ANY ¡æ Death
            new StateTransition(
                State.ANY, State.Death,
                () => mCurrentHp <= 0
            ),
            new StateTransition(
                State.Search, State.MeleeAttack,
                () => IsPlayerInSight(rangeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange
            ),
            new StateTransition(
                State.Idle, State.Patrol,
                () => !stop
            ),
        };
        

    }

    private bool PatrolAble()
    {
        return patrolPoints.Length > 0;
    }

    protected override void Start()
    {
        base.Start();

        fsm = new DataStateMachine(State.Patrol, stateMap, transitions);
        CurrentState = State.Patrol;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
    }

    public override void SetQuestionMark(bool active)
    {
        questionMark.SetActive(active);
    }

    protected override void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Ground") && isDead)
        {
            Explode();
        }
    }
}
