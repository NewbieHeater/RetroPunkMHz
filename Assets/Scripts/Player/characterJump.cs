using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("�ִ� ���� ����")]
    public float maxJumpHeight = 4f;
    [SerializeField, Range(0.2f, 1.25f), Tooltip("�ִ� ���̱��� �ɸ��� �ð�")]
    public float timeToJumpApex;
    [Tooltip("���� ���� Ȱ��ȭ ����")]
    public bool doubleJumpActive = false;
    [Tooltip("���� ���� ��� Ƚ��")]
    public int maxAirJumps;
    [SerializeField, Range(0f, 5f), Tooltip("������ �ö󰥶� �߷�")]
    public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(1f, 10f), Tooltip("������ �����ö� �߷�(������ ���� ������)")]
    public float downwardMovementMultiplier = 6.17f;
    [Tooltip("�ڿ��� Ÿ��: ������ ��� �Ŀ��� ���� �Է��� �޴� �ð�")]
    public float coyoteTime = 0.2f;
    [Tooltip("���� ����: ���� �Է��� �̸� �����ϴ� �ð�")]
    public float jumpBufferTime = 0.2f;
    [SerializeField, Range(1f, 10f), Tooltip("������ �����̽��ٿ��� ���� ������ �߷�")]
    public float jumpCutOff;
    [Tooltip("�ִ� �������� �ӵ�")]
    public float speedLimit = 20f;

    Rigidbody rb;
    bool desiredJump, jumpHeld;
    float coyoteTimer, jumpBufferCounter;
    bool allowDoubleJump, currentlyJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ProcessInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            desiredJump = true;
            jumpHeld = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
            if (rb.velocity.y > 0f)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, 0);
        }
    }

    /// <summary>
    /// �̵� ó�� ����, �߷�/������ velocity�� �����ִ� �޼���
    /// </summary>
    public void HandlePhysics(bool isGrounded)
    {
        // coyote timer
        coyoteTimer = isGrounded ? coyoteTime : (coyoteTimer - Time.fixedDeltaTime);

        // ���� ī����
        if (desiredJump)
        {
            jumpBufferCounter += Time.fixedDeltaTime;
            if (jumpBufferCounter > jumpBufferTime)
                desiredJump = false;
        }

        Vector3 vel = rb.velocity;

        // ���� ���� ����
        if (desiredJump && (isGrounded || coyoteTimer > 0f || allowDoubleJump))
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimer = 0f;

            if (!isGrounded && doubleJumpActive && !allowDoubleJump)
                allowDoubleJump = true;

            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * maxJumpHeight);
            if (vel.y > 0f) jumpSpeed = Mathf.Max(jumpSpeed - vel.y, 0f);
            else vel.y = 0;

            vel.y += jumpSpeed;
            currentlyJumping = true;
        }

        // �߷� ���
        float mult = vel.y > 0
            ? (jumpHeld && currentlyJumping ? upwardMovementMultiplier : jumpCutOff)
            : (isGrounded ? 1f : downwardMovementMultiplier);

        rb.AddForce(new Vector3(0, Physics.gravity.y * (mult - 1f), 0), ForceMode.Acceleration);

        // �ִ� ���� �ӵ� ����
        vel = new Vector3(vel.x, Mathf.Clamp(rb.velocity.y, -speedLimit, float.MaxValue), 0);
        rb.velocity = vel;

        CurrentVelocity = vel;
    }

    public Vector3 CurrentVelocity { get; private set; }
}
