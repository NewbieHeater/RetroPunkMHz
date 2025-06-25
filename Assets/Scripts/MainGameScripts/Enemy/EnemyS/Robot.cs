using EnemyRobotState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeleeAttack
{
    public BoxCollider boxCollider { get; set; }

}

public class Robot : EnemyFSMBase<Robot>, IFireable, IMeleeAttack
{
    public void Fire()
    {
        // 1) 스폰 위치 설정 (머리 위 +1m)
        Vector3 spawnPos = transform.position + Vector3.up * 1f;

        // 2) 플레이어 방향 계산
        Vector3 targetPos = player.transform.position + Vector3.up * 1f; // 플레이어 머리 높이로 조정
        Vector3 dir = (targetPos - spawnPos).normalized;

        // 3) 회전(방향) 설정
        Quaternion rot = Quaternion.LookRotation(dir);

        // 4) 총알 인스턴스화
        Instantiate(bullet, spawnPos, rot);
    }
    [SerializeField] public BoxCollider boxCollider { get; set; }
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
            { State.Search,      new EnemyRobotState.SearchState<Robot>(this)      },
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
                State.RangeAttack, State.Search,
                () => Vector3.Distance(transform.position, player.transform.position) > rangeAttackRange || !IsPlayerInSight(rangeAttackRange)
            ),
            new StateTransition<Robot>(
                State.MeleeAttack, State.Search,
                () => Vector3.Distance(transform.position, player.transform.position) > rangeAttackRange || !IsPlayerInSight(meleeAttackRange)
            ),
            // ANY → Death
            new StateTransition<Robot>(
                State.ANY, State.Death,
                () => currentHp <= 0
            ),
            new StateTransition<Robot>(
                State.Search, State.RangeAttack,
                () => (IsPlayerInSight(rangeAttackRange)
                    || (Vector3.Distance(transform.position, player.transform.position) < findRange
                        && Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange))
            ),
            new StateTransition<Robot>(
                State.Search, State.MeleeAttack,
                () => IsPlayerInSight(meleeAttackRange)
                    || Vector3.Distance(transform.position, player.transform.position) < findRange
            ),
            new StateTransition<Robot>(
                State.Idle, State.Patrol,
                () => !stop
            ),
            new StateTransition<Robot>(
                State.Idle, State.RangeAttack,
                () => (IsPlayerInSight(rangeAttackRange)
                    || (Vector3.Distance(transform.position, player.transform.position) < findRange
                        && Vector3.Distance(transform.position, player.transform.position) > meleeAttackRange))
            ),
        };
        fsm = new DataStateMachine<Robot>(State.Idle, stateMap, transitions);

        boxCollider = GetComponentInChildren<BoxCollider>();
    }

    private bool PatrolAble()
    {
        return patrolPoints.Length > 0;
    }

    protected override void Start()
    {
        
        base.Start();
        
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
