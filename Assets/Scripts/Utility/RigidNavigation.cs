using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidNavigation : MonoBehaviour
{
    public enum MoveMode { Walk, Climb, Jump }
    private MoveMode mode;

    [Header("공통 설정")]
    [SerializeField] float speed = 3f;
    [SerializeField] public float stoppingDistance = 0.01f;

    [Header("Climb 설정")]
    [SerializeField] LayerMask climbableLayer;
    [SerializeField] float wallDetectDistance = 0.5f;
    [SerializeField] float climbSpeed = 2f;

    [Header("Jump 설정")]
    [SerializeField] float jumpApexHeight = 2f;

    public bool hasPath { get; private set; }
    public bool isStopped { get; set; }

    private Vector3 targetPos;
    private Rigidbody rigid;
    private bool jumpLaunched;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        hasPath = false;
        isStopped = false;
        mode = MoveMode.Walk;
        targetPos = transform.position;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // — 일반 걷기
    public void SetDestination(Vector3 dst)
    {
        ResetState();
        mode = MoveMode.Walk;
        targetPos = dst;
        hasPath = true;
    }

    // — 벽 감지 후 자동 등반
    public void SetDestinationClimb(Vector3 dst)
    {
        ResetState();
        mode = MoveMode.Climb;
        targetPos = dst;
        hasPath = true;
    }

    // — 포물선을 그리며 점프
    public void SetDestinationJump(Vector3 dst)
    {
        ResetState();
        mode = MoveMode.Jump;
        targetPos = dst;
        hasPath = true;
        jumpLaunched = false;
    }

    public float RemainingDistance()
        => Vector3.Distance(transform.position, targetPos);

    public void ResetPath()
    {
        ResetState();
        hasPath = false;
    }

    private void ResetState()
    {
        rigid.velocity = Vector3.zero;
        rigid.useGravity = true;
        jumpLaunched = false;
    }

    private void Update()
    {
        if (!hasPath || isStopped) return;

        // 도착 처리
        if (RemainingDistance() <= stoppingDistance && mode != MoveMode.Jump)
        {
            ResetPath();
        }
    }

    private void FixedUpdate()
    {
        if (!hasPath || isStopped)
        {
            rigid.velocity = Vector3.zero;
            return;
        }

        switch (mode)
        {
            case MoveMode.Walk:
                DoWalk();
                break;
            case MoveMode.Climb:
                DoClimb();
                break;
            case MoveMode.Jump:
                DoJump();
                break;
        }
    }

    private void DoWalk()
    {
        float dir = Mathf.Sign(targetPos.x - transform.position.x);
        rigid.velocity = new Vector3(dir * speed, rigid.velocity.y, 0f);
    }

    private void DoClimb()
    {
        float dir = Mathf.Sign(targetPos.x - transform.position.x);
        Vector3 forward = Vector3.right * dir;

        // 벽 감지
        if (Physics.Raycast(transform.position, forward, out var hit, wallDetectDistance, climbableLayer))
        {
            // 등반 중
            rigid.useGravity = false;
            rigid.velocity = Vector3.up * climbSpeed;
        }
        else
        {
            // 등반 끝나면 다시 걷기
            rigid.useGravity = true;
            rigid.velocity = new Vector3(dir * speed, rigid.velocity.y, 0f);
        }
    }

    private void DoJump()
    {
        if (!jumpLaunched)
        {
            // 첫 FixedUpdate에서만 발사
            Vector3 launch = CalculateLaunchVelocity(transform.position, targetPos, jumpApexHeight);
            rigid.velocity = launch;
            jumpLaunched = true;
            rigid.useGravity = true;
        }
        else
        {
            // 공중에서 목표 지점 근처로 오면 경로 종료
            if (RemainingDistance() <= stoppingDistance && Mathf.Abs(rigid.velocity.y) < 0.1f)
                ResetPath();
        }
    }

    // 포물선 계산 (수직+수평 속도)
    private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float apexHeight)
    {
        float g = Physics.gravity.y;
        float vUp = Mathf.Sqrt(-2f * g * apexHeight);
        float tUp = vUp / -g;
        float deltaH = apexHeight - (end.y - start.y);
        float tDown = Mathf.Sqrt(2f * deltaH / -g);
        float totalT = tUp + tDown;

        Vector3 horiz = end - start;
        horiz.y = 0f;
        Vector3 vHoriz = horiz / totalT;

        return vHoriz + Vector3.up * vUp;
    }
}
