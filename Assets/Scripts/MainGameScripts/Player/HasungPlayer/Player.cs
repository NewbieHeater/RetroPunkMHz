﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public struct DamageInfo
{
    public int Amount;          // 입힐 피해량
    public Vector3 SourceDir;     // 공격 원점→목표 방향
    public bool IsCharge;       // 차지 공격 여부
    public float KnockbackForce; // 넉백 세기 (차지 공격일 때만 의미)
}


public class Player : MonoBehaviour
{
    #region Movement Settings
    [Header("Movement Settings")]
    [Tooltip("이동 속도")]
    public float maxSpeed = 5f;
    [Tooltip("가속 시간")]
    public float accelerationTime = 0.1f;
    [Tooltip("감속 시간")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("공중 이동력 (속도 x airControl)")]
    public float airControl = 0.5f;
    [Tooltip("회전 속도")]
    public float rotationSpeed = 0.2f;
    #endregion

    #region Jump Settings
    [Header("Jump Settings")]
    [Tooltip("최대 점프 높이")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f), Tooltip("점프 정점 도달 시간")]
    public float timeToJumpApex = 0.4f;
    [Tooltip("코요테 타임")]
    public float coyoteTime = 0.2f;
    [Tooltip("점프 버퍼 시간")]
    public float jumpBufferTime = 0.2f;
    [Header("Air Jump")]
    [Tooltip("더블 점프 활성화")]
    public bool allowDoubleJump = false;
    [Tooltip("최대 에어 점프 수")]
    public int maxAirJumps = 1;
    [Header("Gravity Multipliers")]
    [SerializeField, Range(0f, 5f), Tooltip("상승 중 중력 배수")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f), Tooltip("하강 중 중력 배수")]
    public float downwardMovementMultiplier = 6.17f;
    [SerializeField, Range(2f, 8f), Tooltip("점프 컷오프 배수")]
    public float jumpCutOff = 2f;
    [Tooltip("떨어지는 속도 제한")]
    public float speedLimit = 15f;
    private float jumpSpeed;
    public bool currentlyJumping = false;
    #endregion

    #region Attack Settings
    [Header("Attack Settings")]
    [Tooltip("일반 공격 데미지")]
    [SerializeField] private int normalDamage = 10;
    [Tooltip("차지 최소/최대 시간")]
    [SerializeField] private float minChargeTime = 0.2f, maxChargeTime = 1f;
    [Tooltip("차지 최대 데미지")]
    [SerializeField] private float maxChargeDamage = 30f;
    [Tooltip("공격 범위")]
    [SerializeField] private float attackRange = 2f;
    [Tooltip("히트박스 반경")]
    [SerializeField] private float attackRadius = 0.5f;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    #endregion

    #region Ground Detection
    [Header("Ground Detection")]
    [Tooltip("바닥 체크 시작점")]
    public Transform groundCheck;
    [Tooltip("바닥 체크 거리")]
    public float groundCheckDistance = 0.5f;
    [Tooltip("바닥 레이어 마스크")]
    public LayerMask groundLayer;
    public bool IsGrounded = false;
    #endregion
    CapsuleCollider cap;
    #region Physics & Calculations
    private Rigidbody rb;
    private Camera cam;
    private Animator animator;
    public Transform playerMesh;

    [Header("Physics Variables")]
    public Vector3 velocity;
    private float coyoteTimer;
    private float jumpBufferCounter;
    [SerializeField] private bool jumpHeld;
    private bool desiredJump;
    private float gravityMultiplier;
    private float defaultGravityScale = 1f;
    [SerializeField]  private float newGravity;
    #endregion

    public int maxHp = 100;
    public int curHp = 0;

    public Image chargeBar;
    public GameObject chargeBarParent;
    public TextMeshProUGUI ChargedValue;
    #region Unity Callbacks
    void Start()
    {
        cap = GetComponent<CapsuleCollider>();
        curHp = maxHp;
        chargeBarParent.SetActive(false);
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        animator = GetComponentInChildren<Animator>();

        allowDoubleJump = false;
        desiredJump = false;
        jumpHeld = false;
    }


    void Update()
    {
        HandleRotation();
        ProcessInput();

        if(!IsGrounded && rb.velocity.y < 0f)
        {
            animator.SetBool("Fall", true);
        }
        else if(IsGrounded)
        {
            animator.SetBool("Fall", false);
            currentlyJumping = false;
        }
    }

    void FixedUpdate()
    {
        RaycastHit groundCheckResult;
        UpdateIsGrounded(out groundCheckResult);
        

        velocity = rb.velocity;
        ApplyGravity();
        UpdateCoyoteTimer();
        ProcessMovement(groundCheckResult);
        
        HandleJumpBufferAndExecute();
        if (desiredJump)
        {
            ExecuteJump();

        }

        //rb.velocity = velocity;
        ApplyPhysics();
        CalculateGravity();
    }
    #endregion
    bool cuted = false;
    float inputX;
    Vector3 lastMoveDir = Vector3.right; // 기본값: 오른쪽
    #region Input Handling
    private void ProcessInput()
    {
        velocity.x = rb.velocity.x;
        inputX = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(inputX) > 0.001f)
        {
            lastMoveDir = new Vector3(inputX, 0f, 0f).normalized;
        }
        //velocity.x = inputX * maxSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            cuted = false ;
            jumpHeld = true;
            jumpBufferCounter = jumpBufferTime;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            
        }

        if(!jumpHeld && !cuted)
        {
            //ApplyJumpCutOff();
            cuted = true;
        }
            

        if (Input.GetMouseButtonDown(0))
            PrimaryAttack();

        if (Input.GetMouseButtonDown(1))
        {
            chargeBarParent.SetActive(true);
            isCharging = true;
            chargeTimer = 0f;
        }
        if (isCharging && Input.GetMouseButton(1))
        {
            if (chargeTimer >= maxChargeTime) return;
            chargeTimer += Time.deltaTime;
            float damage = Mathf.FloorToInt((chargeTimer * 30f) / 10f) * 10;
            ChargedValue.text = damage.ToString();
            chargeBar.fillAmount = chargeTimer / maxChargeTime;
        }
            
        if (isCharging && Input.GetMouseButtonUp(1))
        {
            ChargedAttack(chargeTimer);
            chargeBarParent.SetActive(false);
            chargeBar.fillAmount = 0f;
            isCharging = false;
        }
    }
    #endregion

    #region Movement & Jump

    protected Vector3 AdjustDirectionToSlope(Vector3 direction, RaycastHit groundCheckResult)
    {
        return Vector3.ProjectOnPlane(direction, groundCheckResult.normal).normalized;
    }
    

    bool isOnSlope;
    public bool IsOnSlope(RaycastHit groundCheckResult)
    {
        var angle = Vector3.Angle(Vector3.up, groundCheckResult.normal);
        return angle != 0f && angle < 55;
    }
    private void ProcessMovement(RaycastHit groundCheckResult)
    {
        float dt = Time.fixedDeltaTime;
        // 1) 목표 수평 속도 계산
        float targetVx = inputX * maxSpeed;
        float accel = IsGrounded
            ? maxSpeed / accelerationTime
            : (maxSpeed / accelerationTime) * airControl;
        float decel = IsGrounded
            ? maxSpeed / decelerationTime
            : (maxSpeed / decelerationTime) * airControl;
        Vector3 forward = Vector3.ProjectOnPlane(transform.right, groundCheckResult.normal).normalized;
        //Quaternion footRot = Quaternion.LookRotation(forward, groundCheckResult.normal);

        float newVx = Mathf.Abs(inputX) > 0.01f
            ? Mathf.MoveTowards(velocity.x, targetVx, accel * dt)
            : Mathf.MoveTowards(velocity.x, 0f, decel * dt);
       
        isOnSlope = IsOnSlope(groundCheckResult);

        if (isOnSlope)
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.right, groundCheckResult.normal).normalized;

            float speedMag = Mathf.Abs(newVx);
            velocity = slopeDir * newVx;
        }
        else
        {
            if (wasSlop)
            {
                rb.velocity = new Vector3(newVx, -5f, 0f);
                
            }
            else
            {
                velocity = new Vector3(newVx, rb.velocity.y, 0f);
            }
            
        }
        animator.SetBool("Move", Mathf.Abs(inputX) > 0.01f);
        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        wasSlop = isOnSlope;
        ApplyPhysics();   // rb.velocity = new Vector3(velocity.x, rb.velocity.y, 0)
    }
    bool wasSlop = false;

    /// <summary>
    /// 수평 이동 방향으로 작은 계단이 있으면 그 높이를 stepOffset으로 리턴.
    /// </summary>


    bool jumped = false;

    private void ExecuteJump()
    {
        if (CanJump())
        {
            animator.SetTrigger("JUMP");
            if (!jumpHeld)
                jumpHeld = true;
            desiredJump = false;
            jumpBufferCounter = 0f;
            coyoteTimer = 0f;

            jumpSpeed = Mathf.Sqrt(-1f * 9.81f * upwardMovementMultiplier * newGravity * maxJumpHeight / 5f);

            var currentVerticalSpeed = Vector3.Dot(velocity, Vector3.down);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
   
            velocity.y = (targetVerticalSpeed );

            if (!IsGrounded && maxAirJumps > 0)
                maxAirJumps--;
            currentlyJumping = true;

        }
    }
    private void HandleJumpBufferAndExecute()
    {
        if (!IsGrounded && desiredJump && jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
            if (jumpBufferCounter <= 0f)
            {
                // 버퍼 만료: 더 이상 자동 점프 불가
                jumpBufferCounter = 0f;
                desiredJump = false;
            }
        }

        if (IsGrounded && desiredJump && jumpBufferCounter > 0f)
        {
            ExecuteJump();
            // 버퍼 사용했으므로 초기화
            jumpBufferCounter = 0f;
            desiredJump = false;
        }
    }

    private bool CanJump()
    {
        return IsGrounded || coyoteTimer > 0f || (allowDoubleJump && maxAirJumps > 0);
    }

    private void ApplyJumpCutOff()
    {
        if (rb.velocity.y > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0f);
            Debug.Log("cut");
        }
            
    }

    private void UpdateCoyoteTimer()
    {
        if (IsGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime;
    }
    #endregion

    #region Gravity & Physics
    private void CalculateGravity()
    {
        if (rb.velocity.y > 0.01f)
        {
            if (IsGrounded)
            {
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                if (jumpHeld)
                {
                    gravityMultiplier = upwardMovementMultiplier;
                }
                else
                {
                    //gravityMultiplier = jumpCutOff;
                }
            }
        }
        else if (rb.velocity.y < 0.01f)
        {
            if (IsGrounded)
            {
                allowDoubleJump = false;
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                gravityMultiplier = downwardMovementMultiplier;
            }
        }
        else if (!jumpHeld && IsGrounded)
        {
            gravityMultiplier = defaultGravityScale;
        }
        else
        {
            if (IsGrounded)
            {
                currentlyJumping = false;
            }

            gravityMultiplier = defaultGravityScale;
        }
        //rb.velocity = new Vector3(velocity.x, Mathf.Clamp(rb.velocity.y, -speedLimit, 100), 0);
    }
    public float a;
    private void ApplyGravity()
    {
        //newGravity = (((-2 * maxJumpHeight) / (timeToJumpApex * timeToJumpApex)) / 9.81f);
        if (IsGrounded || isOnSlope)
        {
            return;
        }
        newGravity = (((-2) / (timeToJumpApex * timeToJumpApex)));
        a = newGravity * gravityMultiplier;
        rb.AddForce(Vector3.up * newGravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void ApplyPhysics()
    {
        //float mul = rb.velocity.y > 0 ? upwardMovementMultiplier : downwardMovementMultiplier;
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, float.MaxValue), 0f);
    }
    #endregion

    #region Attack

    private Vector3 GetLastInputDirection()
    {
        // 만약 lastMoveDir이 벡터 (±1,0,0)이어서 공격의 XY 평면 방향으로 적절히 사용됩니다.
        // 그 외에도 필요 시 y축으로 변환한다면 여기에 로직 추가 가능
        Vector3 dir = lastMoveDir;
        dir.z = 0f;
        return dir.normalized;
    }

    public void PrimaryAttack()
    {
        animator.SetTrigger("Attack");
        var info = new DamageInfo
        {
            Amount = normalDamage,
            SourceDir = GetAttackDirection(),
            IsCharge = false,
            KnockbackForce = 0
        };
        ExecuteAttack(info);
    }
        
    public void ChargedAttack(float chargeTime)
    {
        animator.Play("Attack_5Combo_4_Inplace");
        float t = Mathf.Clamp(chargeTimer, minChargeTime, maxChargeTime);
        int dmg = (int)Mathf.Lerp(normalDamage, maxChargeDamage, (t - minChargeTime) / (maxChargeTime - minChargeTime));

        var info = new DamageInfo
        {
            Amount = dmg,
            SourceDir = GetAttackDirection(),
            IsCharge = t >= minChargeTime,
            KnockbackForce = dmg
        };
        ExecuteAttack(info);

    }
    private Collider[] _overlapResults = new Collider[16];

    float attackCapsuleHeight = 1f;
    private void ExecuteAttack(in DamageInfo info)
    {
        Vector3 dir = GetLastInputDirection(); // lastMoveDir이 들어있음
        Vector3 tipCenter = transform.position + Vector3.up + dir * attackRange / 2;
        attackRadius = attackRange / 2;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f).normalized;
        float halfHeight = attackCapsuleHeight * 0.5f;

        Vector3 pointA = tipCenter + perp * halfHeight;
        Vector3 pointB = tipCenter - perp * halfHeight;

        int mask = LayerMask.GetMask("Enemy", "Destructible");
        int hitCount = Physics.OverlapCapsuleNonAlloc(
            pointA,
            pointB,
            attackRadius,
            _overlapResults,
            mask,
            QueryTriggerInteraction.Collide
        );
        Debug.Log($"OverlapCapsule hitCount = {hitCount}");
        for (int i = 0; i < hitCount; i++)
        {
            if (_overlapResults[i].TryGetComponent<IAttackable>(out var atk))
                atk.TakeDamage(info);
        }
    }
#if UNITY_EDITOR
    // Editor뷰에서 공격 캡슐을 시각화해 볼 수 있는 코드
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 dir = GetLastInputDirection();
        Vector3 tipCenter = transform.position + Vector3.up + dir * attackRange / 2;
        float halfHeight = attackCapsuleHeight * 0.5f;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f).normalized;
        Vector3 pointA = tipCenter + perp * halfHeight;
        Vector3 pointB = tipCenter - perp * halfHeight;
        float radius = attackRadius;

        // 캡슐의 끝점에 와이어스피어
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointA, radius);
        Gizmos.DrawWireSphere(pointB, radius);

        // 중앙을 잇는 선
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pointA, pointB);
    }
#endif



    private Vector3 GetAttackDirection()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 world = cam.ScreenToWorldPoint(mp);
        Vector3 dir = (world - transform.position);
        dir.z = 0f;
        return dir.normalized;
    }
    #endregion
    bool falling = false;
    #region HpModify

    public void ModifyHp(int atkPower)
    {
        curHp -= atkPower;
    }

    #endregion
    public RaycastHit LastGroundHit { get; private set; }
    [Header("Ground Check")]
    [SerializeField] float checkHeight = 0.1f;
    [SerializeField] float checkRadius = 0.5f;
    [SerializeField] float maxFallDistance = 0.3f;
    #region Ground Check
    private bool UpdateIsGrounded(out RaycastHit hit)
    {
        // 2) CheckSphere 로 대략적 접지 체크
        Vector3 origin = transform.position + Vector3.up * checkHeight;
        bool grounded = Physics.CheckSphere(
            origin,
            checkRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        hit = default;
        // 3) 실제 접촉면 노멀·위치 정보가 필요하면 Raycast
        if (grounded)
        {
            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit rHit,
                    checkHeight + maxFallDistance,
                    groundLayer,
                    QueryTriggerInteraction.Ignore))
            {
                hit = rHit;
            }
        }
        


        animator.SetBool("Grounded", grounded);

        IsGrounded = grounded;
        LastGroundHit = hit;
        return grounded;
    }
    #endregion

    #region Utilities
    private void HandleRotation()
    {
        if (Mathf.Abs(velocity.x) > 0.01f)
        {
            playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation,
                Quaternion.LookRotation(Vector3.right * velocity.x), rotationSpeed);
            if (!jumped && IsGrounded)
            {
                animator.SetBool("Move", true);
                animator.SetBool("Idle", false);
            }
            
        }
        else
        {
            if (!jumped && IsGrounded)
            {
                animator.SetBool("Move", false);
                animator.SetBool("Idle", true);
            }
            
        }
    }
    #endregion

}
