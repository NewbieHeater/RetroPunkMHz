using System.Collections;
using System.Collections.Generic;
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
    private bool currentlyJumping = false;
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
    [SerializeField] protected CharacterMotorConfig Config;
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

    #region Physics & Calculations
    private Rigidbody rb;
    private Camera cam;
    private Animator animator;
    public Transform playerMesh;

    [Header("Physics Variables")]
    public Vector3 velocity;
    private float coyoteTimer;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool desiredJump;
    private float gravityMultiplier;
    private float defaultGravityScale = 1f;
    private float newGravity;
    #endregion

    public Image HpBar;
    public GameObject HpBarParent;
    public TextMeshProUGUI ChargedValue;
    #region Unity Callbacks
    void Start()
    {
        HpBarParent.SetActive(false);
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
        
        HandleJumpBuffer();
        if(rb.velocity.y < -1f && !IsGrounded)
        {
            animator.Play("Unarmed-Fall");
        }
    }

    void FixedUpdate()
    {
        RaycastHit groundCheckResult = UpdateIsGrounded();


        velocity = rb.velocity;
        ApplyGravity();
        UpdateCoyoteTimer();
        ProcessMovement(groundCheckResult);


        if (desiredJump)
        {
            ExecuteJump();

        }

        //rb.velocity = velocity;
        ApplyPhysics();
        CalculateGravity();
    }
    #endregion
    float inputX;
    #region Input Handling
    private void ProcessInput()
    {
        velocity.x = rb.velocity.x;
        inputX = Input.GetAxisRaw("Horizontal");
        //velocity.x = inputX * maxSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            jumpHeld = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            ApplyJumpCutOff();
        }

        if (Input.GetMouseButtonDown(0))
            PrimaryAttack();

        if (Input.GetMouseButtonDown(1))
        {
            HpBarParent.SetActive(true);
            isCharging = true;
            chargeTimer = 0f;
        }
        if (isCharging && Input.GetMouseButton(1))
        {
            if (chargeTimer >= maxChargeTime) return;
            chargeTimer += Time.deltaTime;
            float damage = Mathf.FloorToInt((chargeTimer * 30f) / 10f) * 10;
            ChargedValue.text = damage.ToString();
            HpBar.fillAmount = chargeTimer / maxChargeTime;
        }
            
        if (isCharging && Input.GetMouseButtonUp(1))
        {
            ChargedAttack(chargeTimer);
            HpBarParent.SetActive(false);
            HpBar.fillAmount = 0f;
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
        wasSlop = isOnSlope;
        ApplyPhysics();   // rb.velocity = new Vector3(velocity.x, rb.velocity.y, 0)
    }
    bool wasSlop = false;

    /// <summary>
    /// 수평 이동 방향으로 작은 계단이 있으면 그 높이를 stepOffset으로 리턴.
    /// </summary>
    private bool TryStepOffset(Vector3 horizontalMove, out float stepOffset)
    {
        stepOffset = 0f;

        Vector3 dir = horizontalMove.normalized;
        float halfH = Config.StepCheck_MaxStepHeight * 0.5f;
        float fullH = Config.StepCheck_MaxStepHeight;
        float lookD = Config.Radius + Config.StepCheck_LookAheadRange;
        int mask = Config.GroundedLayerMask;

        // 1) 앞에 장애물 있는지
        Vector3 origin1 = transform.position + Vector3.up * halfH;
        if (!Physics.Raycast(origin1, dir, lookD, mask))
            return false;

        // 2) 장애물 위 공간이 비어있는지
        Vector3 origin2 = transform.position + Vector3.up * fullH;
        if (Physics.Raycast(origin2, dir, lookD, mask))
            return false;

        // 3) 계단 표면 높이 검사
        Vector3 rayOrigin = origin2 + dir * lookD;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, fullH * 2f, mask))
        {
            // 너무 가파르면 무시
            if (Vector3.Angle(hit.normal, Vector3.up) > Config.SlopeLimit)
                return false;

            // 계단 높이만큼 보정
            float diffY = hit.point.y - transform.position.y;
            if (diffY > 0f && diffY <= Config.StepCheck_MaxStepHeight)
            {
                stepOffset = diffY + 0.01f;  // 소폭 여유 추가
                return true;
            }
        }

        return false;
    }

    bool jumped = false;

    private void ExecuteJump()
    {
        if (CanJump())
        {
            animator.Play("Unarmed-Jump");
            jumped = true;
            desiredJump = false;
            jumpBufferCounter = 0f;
            coyoteTimer = 0f;

            jumpSpeed = Mathf.Sqrt(-1f * 9.81f * upwardMovementMultiplier * newGravity * maxJumpHeight / 5f);

            var currentVerticalSpeed = Vector3.Dot(velocity, Vector3.down);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

            velocity += Vector3.up * (targetVerticalSpeed + currentVerticalSpeed);

            if (!IsGrounded && maxAirJumps > 0)
                maxAirJumps--;
            currentlyJumping = true;
        }
        if (jumpBufferTime == 0)
        {
            desiredJump = false;
        }
    }
    private void HandleJumpBuffer()
    {
        if (!jumpBufferTime.Equals(0) && desiredJump)
        {
            jumpBufferCounter += Time.deltaTime;
            if (jumpBufferCounter > jumpBufferTime)
            {
                desiredJump = false;
                jumpBufferCounter = 0f;
            }
        }
    }
    private bool CanJump()
    {
        return IsGrounded || coyoteTimer > 0f || (allowDoubleJump && maxAirJumps > 0);
    }

    private void ApplyJumpCutOff()
    {
        if (rb.velocity.y > 0f)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0f);
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
                if (jumpHeld && currentlyJumping)
                {
                    gravityMultiplier = upwardMovementMultiplier;
                }
                else
                {
                    gravityMultiplier = jumpCutOff;
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
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(rb.velocity.y, -speedLimit, 100), 0);
    }
    
    private void ApplyGravity()
    {
        //newGravity = (((-2 * maxJumpHeight) / (timeToJumpApex * timeToJumpApex)) / 9.81f);
        if (IsGrounded || isOnSlope)
        {
            return;
        }
        newGravity = (((-2) / (timeToJumpApex * timeToJumpApex)));
        rb.AddForce(Vector3.up * newGravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void ApplyPhysics()
    {
        //float mul = rb.velocity.y > 0 ? upwardMovementMultiplier : downwardMovementMultiplier;
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, float.MaxValue), 0f);
    }
    #endregion

    #region Attack
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
            KnockbackForce = dmg / 3f
        };
        ExecuteAttack(info);

    }
    private void ExecuteAttack(in DamageInfo info)
    {
        var dir = info.SourceDir;
        var hits = Physics.SphereCastAll(
            transform.position,
            attackRadius,
            dir,
            attackRange,
            LayerMask.GetMask("Enemy", "Destructible")
        );

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<IAttackable>(out var atk))
                atk.TakeDamage(info);
        }
    }


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
    
    float Radius;
    float GroundedCheckRadiusBuffer;
    #region Ground Check
    protected RaycastHit UpdateIsGrounded()
    {
        RaycastHit hitResult;

        // currently performing a jump
        if (jumpBufferCounter > 0)
        {
            IsGrounded = false;
            return new RaycastHit();
        }

        Vector3 startPos = transform.position + Vector3.up * 0.5f;
        float groundCheckRadius = Radius + GroundedCheckRadiusBuffer;
        float groundCheckDistance = 0.8f;

        // perform our spherecast
        if (Physics.SphereCast(startPos, groundCheckRadius, Vector3.down, out hitResult,
                               groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {

            IsGrounded = true;
            if (jumped && IsGrounded)
            {
                animator.Play("Unarmed-Land");
                jumped = false;
            }
            // add auto parenting here
        }
        else
            IsGrounded = false;

        return hitResult;
    }
    #endregion
    
    #region Utilities
    private void HandleRotation()
    {
        if (Mathf.Abs(velocity.x) > 0.01f)
        {
            playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation,
                Quaternion.LookRotation(Vector3.right * velocity.x), rotationSpeed);
            if(!jumped && IsGrounded)
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
