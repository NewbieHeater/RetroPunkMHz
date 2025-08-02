using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody))]
public class RigidNavigation : MonoBehaviour
{
    public enum MoveMode { Walk, Climb, Jump }
    private MoveMode mode;

    [Header("공통 설정")]
    [SerializeField] private float speed = 3f;
    [SerializeField] public float stoppingDistance = 0.01f;

    [Header("Climb 설정")]
    [SerializeField] private LayerMask climbableLayer;
    [SerializeField] private float wallDetectDistance = 0.1f;
    [SerializeField] private float climbSpeed = 2f;

    [Header("Jump 설정")]
    [SerializeField] private float jumpApexHeight = 30f;

    //플래그 변수
    public bool hasPath { get; private set; }
    public bool isStopped { get; set; }
    public bool isGrounded { get; private set; }
    private bool jumpLaunched;

    //내부 변수
    private Vector3 resetVector = Vector3.zero;
    private Vector3 targetPos;

    private Rigidbody rigid;
    

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

    private void SetDestination(Vector3 dst, MoveMode moveMode, System.Action extraSetup = null)
    {
        ResetState();
        mode = moveMode;
        targetPos = dst;
        extraSetup?.Invoke();
        ResetOnMove();
    }

    public void SetDestinationWalk(Vector3 dst)
    {
        SetDestination(dst, MoveMode.Walk);
    }

    public void SetDestinationClimb(Vector3 dst)
    {
        SetDestination(dst, MoveMode.Climb);
    }

    public void SetDestinationJump(Vector3 dst)
    {
        SetDestination(dst, MoveMode.Jump, () => jumpLaunched = false);
    }


    private void ResetOnMove()
    {
        hasPath = true;
        isStopped = false;
        isReset = false;
    }

    public float RemainingDistance()
        => Mathf.Abs(transform.position.x - targetPos.x);

    public void ResetPath()
    {
        ResetState();
        hasPath = false;
    }

    private void ResetState()
    {
        //rigid.velocity = resetVector;
        rigid.useGravity = true;
        jumpLaunched = false;
        isReset = true;
    }
    [SerializeField] private bool isReset = false;

    private void Update()
    {
        resetVector = new Vector3(0,rigid.velocity.y,0);
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 0.7f, climbableLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (!hasPath || isStopped) return;

        // 도착 처리
        if (RemainingDistance() <= stoppingDistance && mode != MoveMode.Jump && !isReset && isGrounded)
        {
            
            ResetPath();
        }
    }
    
    private void FixedUpdate()
    {
        if (!hasPath || isStopped)
        {
            rigid.velocity = resetVector;
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
        Vector3 move = new Vector3(dir * speed * Time.fixedDeltaTime, 0f, 0f);
        rigid.MovePosition(rigid.position + move);
    }
    private float rotationSpeed = 180f;
    [SerializeField] float wallOffset = 0.05f;
    private void DoClimb()
    {
        float dir = Mathf.Sign(targetPos.x - transform.position.x);
        Vector3 forward = Vector3.right * dir;
        
        // 벽 감지
        if (Physics.Raycast(transform.position + Vector3.down * 0.5f, forward, out var hit, wallDetectDistance, climbableLayer))
        {
            
            Quaternion targetRot = Quaternion.Euler(0f, 0f, Vector3.ProjectOnPlane(forward, hit.normal).z + 90);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );

            rigid.useGravity = false;
            float newY = Mathf.MoveTowards(
                transform.position.y,
                targetPos.y,
                climbSpeed * Time.fixedDeltaTime
            );
            float fixX = hit.point.x - forward.x * wallOffset;
            Vector3 newPos = new Vector3(fixX, newY, 0f);
            rigid.MovePosition(newPos);
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.identity,         // 세계 기준 앞(필요하면 originalRotation으로)
            rotationSpeed * Time.deltaTime
            );
            //rigid.useGravity = true;
            DoWalk();
        }
    }

    private void DoJump()
    {
        if (!jumpLaunched)
        {
            Vector3 launch = CalculateLaunchVelocity(transform.position, targetPos, jumpApexHeight);

            rigid.velocity = launch;
            jumpLaunched = true;
            rigid.useGravity = true;
        }
        else
        {
            // 공중에서 목표 지점 근처로 오면 경로 종료
            if (RemainingDistance() <= stoppingDistance && isGrounded)
                ResetPath();
            else if(RemainingDistance() >= stoppingDistance)
                SetDestinationWalk(targetPos);
        }
    }

    // 포물선 계산 (수직+수평 속도)
    private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float heightBias = 1.5f)
    {
        float g = Physics.gravity.y;

        // 시작점과 도착점의 높이 차이
        float deltaY = end.y - start.y;

        // 최고점 높이 계산: 더 높은 곳을 향할수록 더 높은 점프가 필요함
        float apexHeight = Mathf.Max(deltaY + heightBias, heightBias); // 항상 최소 heightBias 이상

        // 수직 속도 및 상승 시간 계산
        float vUp = Mathf.Sqrt(-2f * g * apexHeight);
        float tUp = vUp / -g;

        // 하강 거리와 시간 계산
        float tDown = Mathf.Sqrt(2f * Mathf.Max(apexHeight - deltaY, 0.01f) / -g);
        float totalT = tUp + tDown;

        // 수평 속도 계산
        Vector3 horizontal = end - start;
        horizontal.y = 0f;
        Vector3 vHoriz = horizontal / totalT;

        return vHoriz + Vector3.up * vUp;
    }

}
