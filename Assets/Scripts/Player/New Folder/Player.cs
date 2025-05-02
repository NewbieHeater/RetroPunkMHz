using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float normalDamage = 10f;
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
    public bool isGrounded { get; private set; } = false;
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

    #region Unity Callbacks
    void Start()
    {
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
        GroundCheck();
        HandleJumpBuffer();
    }

    void FixedUpdate()
    {
        ApplyGravity();
        
        velocity = rb.velocity;
        ApplyPhysics();
        UpdateCoyoteTimer();
        ProcessMovement();

        if (desiredJump)
        {
            ExecuteJump();
            rb.velocity = velocity;
            return;
        }

        rb.velocity = velocity;
        CalculateGravity();
    }
    #endregion

    #region Input Handling
    private void ProcessInput()
    {
        velocity.x = rb.velocity.x;
        float inputX = Input.GetAxisRaw("Horizontal");
        velocity.x = inputX * maxSpeed;

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
            isCharging = true;
            chargeTimer = 0f;
        }
        if (isCharging && Input.GetMouseButton(1))
            chargeTimer += Time.deltaTime;
        if (isCharging && Input.GetMouseButtonUp(1))
        {
            ChargedAttack(chargeTimer);
            isCharging = false;
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
    #endregion

    #region Movement & Jump
    private void ProcessMovement()
    {
        float targetX = velocity.x;
        float currentX = rb.velocity.x;
        bool grounded = isGrounded;

        float accel = grounded ? maxSpeed / accelerationTime : maxSpeed / accelerationTime * airControl;
        float decel = grounded ? maxSpeed / decelerationTime : maxSpeed / decelerationTime * airControl;

        velocity.x = Mathf.MoveTowards(currentX, targetX, (Mathf.Abs(targetX) > 0.01f ? accel : decel) * Time.fixedDeltaTime);
    }
    
    private void ExecuteJump()
    {
        if (CanJump())
        {
            desiredJump = false;
            jumpBufferCounter = 0f;
            coyoteTimer = 0f;

            jumpSpeed = Mathf.Sqrt(-1f * 9.81f * upwardMovementMultiplier * newGravity * maxJumpHeight / 5f);

            var currentVerticalSpeed = Vector3.Dot(velocity, Vector3.down);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

            velocity += Vector3.up * (targetVerticalSpeed + currentVerticalSpeed);

            if (!isGrounded && maxAirJumps > 0)
                maxAirJumps--;
            currentlyJumping = true;
        }
        if (jumpBufferTime == 0)
        {
            desiredJump = false;
        }
    }
    
    private bool CanJump()
    {
        return isGrounded || coyoteTimer > 0f || (allowDoubleJump && maxAirJumps > 0);
    }

    private void ApplyJumpCutOff()
    {
        if (rb.velocity.y > 0f)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0f);
    }

    private void UpdateCoyoteTimer()
    {
        if (isGrounded)
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
            if (isGrounded)
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
            if (isGrounded)
            {
                allowDoubleJump = false;
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                gravityMultiplier = downwardMovementMultiplier;
            }
        }
        else if (!jumpHeld && isGrounded)
        {
            gravityMultiplier = defaultGravityScale;
        }
        else
        {
            if (isGrounded)
            {
                currentlyJumping = false;
            }

            gravityMultiplier = defaultGravityScale;
        }
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(rb.velocity.y, -speedLimit, 100), 0);
    }
    
    private void ApplyPhysics()
    {
        newGravity = (((-2 * maxJumpHeight) / (timeToJumpApex * timeToJumpApex)) / 9.81f);

        rb.AddForce(Vector3.up * newGravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void ApplyGravity()
    {
        //float mul = rb.velocity.y > 0 ? upwardMovementMultiplier : downwardMovementMultiplier;
        rb.velocity = new Vector3(velocity.x, Mathf.Clamp(rb.velocity.y, -speedLimit, float.MaxValue), 0f);
    }
    #endregion

    #region Attack
    public void PrimaryAttack()
        => ProcessAttack(normalDamage, false);

    public void ChargedAttack(float chargeTime)
    {
        float damage = (chargeTime >= minChargeTime) ? maxChargeDamage : normalDamage;
        bool knockback = (chargeTime >= minChargeTime);
        ProcessAttack(damage, knockback);
    }
    private void ProcessAttack(float damage, bool knockback)
    {
        Vector3 dir = GetAttackDirection();
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, attackRadius, dir, attackRange, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy == null) continue;

            enemy.TakeDamage(damage);
            if (enemy.isDead() && knockback)
                enemy.ApplyKnockback(dir * 20f);
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

    #region Ground Check
    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    #endregion

    #region Utilities
    private void HandleRotation()
    {
        if (Mathf.Abs(velocity.x) > 0.01f)
            playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation,
                Quaternion.LookRotation(Vector3.right * velocity.x), rotationSpeed);
    }
    #endregion
}
