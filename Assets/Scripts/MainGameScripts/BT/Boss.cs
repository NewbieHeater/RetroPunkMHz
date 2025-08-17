using UnityEngine;
using UnityEngine.AI; // NavMesh를 사용하는 예시 (선택)


public class Boss : BTRunner
{
    [Header("Refs")]
    public Transform[] patrolPoints;
    public Transform target;
    public NavMeshAgent agent;

    [Header("Tuning")]
    public float sightRange = 10f;
    public float attackRange = 2f;
    public float repathInterval = 0.25f;

    private int _patrolIndex;

    protected override void Awake()
    {
        base.Awake();
        if (!agent) agent = GetComponent<NavMeshAgent>();
    }

    protected override BTNode BuildTree()
    {
        // 블랙보드 초기값 세팅
        Blackboard.Set("hasTarget", false);
        Blackboard.Set("lastRepath", 0f);

        bool HasTarget(BTContext ctx) => target != null && Vector3.Distance(transform.position, target.position) <= sightRange;
        bool InAttackRange(BTContext ctx) => target != null && Vector3.Distance(transform.position, target.position) <= attackRange;

        return new BTBuilder()
            .Selector("RootSelector")
                // 1) 타겟 있으면 교전 로직
                .Guard(HasTarget, b =>
                {
                    b.Selector("Engage")
                        // 1-1) 공격 범위면 공격
                        .Guard(InAttackRange, gb =>
                        {
                            gb.Do("Attack", AttackTick);
                        })
                        // 1-2) 아니면 추격
                        .Do("Chase", ChaseTick);
                })
                // 2) 타겟 없으면 순찰
                .Sequence("Patrol")
                    .Do("PickNextPatrolPoint", PickNextPatrolPoint)
                    .Do("MoveToPatrolPoint", MoveToPatrolPoint)
                    .Wait(0.2f)
                .End()
            .End()
            .Build();
    }

    // ---- 액션들 ----
    private NodeStatus AttackTick(BTContext ctx)
    {
        if (target == null) return NodeStatus.Failure;

        // 여기서 애니메이션/데미지 트리거 등 처리
        // 쿨다운 예시: 공격이 끝나면 잠깐 쉬게 하고 싶다면 Cooldown 데코레이터로 감싸면 됨.
        Debug.DrawLine(transform.position, target.position, Color.red, 0.05f);
        return NodeStatus.Success; // 한 번의 공격을 Success로 보고 상위에서 반복 호출할 수 있음
    }

    private NodeStatus ChaseTick(BTContext ctx)
    {
        if (target == null || agent == null) return NodeStatus.Failure;

        float last = ctx.Blackboard.Get("lastRepath", 0f);
        if (Time.time - last >= repathInterval)
        {
            agent.SetDestination(target.position);
            ctx.Blackboard.Set("lastRepath", Time.time);
        }

        bool reached = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        return reached ? NodeStatus.Success : NodeStatus.Running;
    }

    private NodeStatus PickNextPatrolPoint(BTContext ctx)
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || agent == null) return NodeStatus.Failure;
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[_patrolIndex].position);
        return NodeStatus.Success;
    }

    private NodeStatus MoveToPatrolPoint(BTContext ctx)
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) return NodeStatus.Failure;
        bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        return arrived ? NodeStatus.Success : NodeStatus.Running;
    }

    // 디버그로 시야/공격 범위를 그려보면 좋아요
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
