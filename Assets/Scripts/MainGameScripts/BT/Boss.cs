using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boss : BTRunner
{
    [Header("Refs")]
    public Transform[] patrolPoints;
    public Transform target;
    private Rigidbody rb;

    [Header("Move Tuning")]
    public float moveSpeed = 3.5f;     // m/s
    public float turnSpeedDeg = 540f;  // 초당 회전 각도(도)
    public float arriveDist = 0.15f;   // 순찰 포인트 도착 판정 거리
    public float waitAtPoint = 0.25f;  // 포인트 도착 후 잠시 쉬기
    public float chaseRepath = 0.15f;  // 추격 중 목표 재설정 주기(초)

    [Header("Perception")]
    public float sightRange = 12f;
    public float attackRange = 2.2f;

    // 내부 상태
    private int _patrolIndex = -1;
    private Vector3 _moveTarget;
    private float _lastChaseRepath;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 부드럽게
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // 수평 회전만
        Blackboard.Set("lastChaseRepath", 0f);
    }

    protected override BTNode BuildTree()
    {
        bool HasTarget(BTContext ctx)
            => target && Vector3.Distance(transform.position, target.position) <= sightRange;

        bool InAttackRange(BTContext ctx)
            => target && Vector3.Distance(transform.position, target.position) <= attackRange;

        return new BTBuilder()
            .Selector("Root")
                // 1) 타겟 감지 시 교전
                .Guard(HasTarget, b =>
                {
                    b.Selector("Engage")
                        // 1-1) 사정거리면 공격
                        .Guard(InAttackRange, g =>
                        {
                            g.Do("Attack", onTick: AttackTick, onStart: AttackStart, onStop: AttackStop);
                        })
                        // 1-2) 아니면 추격(Rigidbody 이동)
                        .Do("Chase", onTick: ChaseTick, onStart: ChaseStart, onStop: ChaseStop)
                        .End();
                })
                // 2) 타겟 없으면 순찰
                .Sequence("Patrol")
                    .Do("PickNextPatrolPoint", PickNextPatrolPoint)
                    .Do("MoveToPatrolPoint", onTick: MoveToPointTick, onStart: MoveToPointStart, onStop: MoveToPointStop)
                    .Wait(waitAtPoint)
                .End()
            .End()
            .Build();
    }

    // ---------- 공통 유틸 ----------
    private void FaceTowards(Vector3 worldPos, float dt)
    {
        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 1e-6f) return;

        Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeedDeg * dt);
    }

    // Rigidbody.MovePosition을 이용한 평면 이동
    // 반환값: 도착했으면 true
    private bool MoveTowardsXZ(Vector3 worldPos, float speed, float arrive, float dt)
    {
        Vector3 to = worldPos - transform.position;
        Vector3 horiz = new Vector3(to.x, 0f, to.z);
        float dist = horiz.magnitude;

        if (dist > 1e-4f) FaceTowards(worldPos, dt);
        if (dist <= arrive) return true;

        Vector3 stepDir = (dist > 1e-6f) ? horiz / dist : Vector3.zero;
        float step = Mathf.Min(dist, speed * dt);
        Vector3 next = rb.position + stepDir * step;

        rb.MovePosition(next);
        return false;
    }

    // ---------- 순찰 ----------
    private NodeStatus PickNextPatrolPoint(BTContext ctx)
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return NodeStatus.Failure;
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        _moveTarget = patrolPoints[_patrolIndex].position;
        return NodeStatus.Success;
    }

    private void MoveToPointStart(BTContext ctx)
    {
        // 필요 시 애니/이펙트 시작 등
    }

    private NodeStatus MoveToPointTick(BTContext ctx)
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return NodeStatus.Failure;
        bool arrived = MoveTowardsXZ(_moveTarget, moveSpeed, arriveDist, ctx.DeltaTime);
        return arrived ? NodeStatus.Success : NodeStatus.Running;
    }

    private void MoveToPointStop(BTContext ctx, NodeStatus result)
    {
        // 필요 시 정리
    }

    // ---------- 추격 ----------
    private void ChaseStart(BTContext ctx)
    {
        Debug.Log("ChaseStart");
        _lastChaseRepath = Blackboard.Get("lastChaseRepath", 0f);
        if (target) _moveTarget = target.position; // 시작 즉시 한 번 목표 세팅
    }

    private NodeStatus ChaseTick(BTContext ctx)
    {
        if (!target) return NodeStatus.Failure;

        // 일정 주기로 목표 재설정 (타겟 급변시 흔들림 완화)
        if (Time.time - _lastChaseRepath >= chaseRepath)
        {
            _moveTarget = target.position;
            _lastChaseRepath = Time.time;
            Blackboard.Set("lastChaseRepath", _lastChaseRepath);
        }

        bool near = MoveTowardsXZ(_moveTarget, moveSpeed, attackRange * 0.9f, ctx.DeltaTime);
        // 사정거리 근접을 '성공'으로 보고 상위 Selector가 공격 가지를 평가하게 함
        return near ? NodeStatus.Success : NodeStatus.Running;
    }

    private void ChaseStop(BTContext ctx, NodeStatus result)
    {
        // 추격 종료/중단 시 정리 필요하면 여기에
    }

    // ---------- 공격 ----------
    private void AttackStart(BTContext ctx)
    {
        Debug.Log("BossAttack");
        if (target) FaceTowards(target.position, ctx.DeltaTime);
        anime.ResetTrigger("Attack");
        anime.SetTrigger("Attack");
    }

    private NodeStatus AttackTick(BTContext ctx)
    {
        if (!target) return NodeStatus.Failure;

        Debug.DrawLine(transform.position, target.position, Color.red, 0.05f);

        FaceTowards(target.position, ctx.DeltaTime);

        var st = anime.GetCurrentAnimatorStateInfo(0);
        bool inAttack = st.IsName("Attack");

        // 아직 공격 애니로 못들어갔으면 계속 대기
        //if (!inAttack) return NodeStatus.Running;

        // 애니메이션 진행이 끝났다면 성공
        //if (st.normalizedTime >= 0.95f) return NodeStatus.Success;
        if (!inAttack) return NodeStatus.Success;
        return NodeStatus.Running;
    }

    private void AttackStop(BTContext ctx, NodeStatus result)
    {
        // 쿨다운 세팅/판정 해제 등
        //Blackboard.Set("nextAttackTime", Time.time + 1.0f);
    }

    // ---------- 디버그 ----------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan; Gizmos.DrawSphere(_moveTarget, 0.08f);
    }
    [SerializeField] private RigidPlayerManagement player;
    [SerializeField] private Animator anime;
    [SerializeField] private float viewAngle;
    [SerializeField] private float maxDist;
    [SerializeField] private LayerMask obstacleMask;

    protected bool IsPlayerInSight(float range)
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > range)
            return false;

        Vector3 toPlayer = (player.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(anime.transform.forward, toPlayer);
        Debug.DrawRay(transform.position + Vector3.up, toPlayer * dist, Color.green, 0.1f);
        if (angle > viewAngle)
        {
            Debug.DrawRay(transform.position + Vector3.up, toPlayer * dist, Color.gray, 0.1f);
            return false;
        }

        Vector3 origin = transform.position + Vector3.up;
        Vector3 target = player.transform.position + Vector3.up;
        Vector3 dir = target - origin;
        Debug.DrawRay(origin, dir.normalized * range, Color.red, 0.1f);

        return IsRayHitOnPlayer(origin, dir);
    }

    public RaycastHit? GetRaycastHit(Vector3 origin, Vector3 dir, float distance)
    {
        Debug.DrawRay(origin, dir.normalized * distance, Color.red, 0.1f);

        if (Physics.Raycast(origin, dir, out var hit, distance, obstacleMask))
        {
            return hit;
        }

        return null;
    }

    public bool IsRayHitOnPlayer(Vector3 origin, Vector3 dir)
    {
        if (GetRaycastHit(origin, dir, maxDist) is RaycastHit hit)
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }
}
