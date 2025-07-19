using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class RigidMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxWalkSpeed = 5f;
    public float maxRunSpeed = 8f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.2f;
    [Range(0f, 1f)] public float airControl = 0.5f;
    public float rotationSpeed = 0.2f;

    private Rigidbody rb;
    private Animator animator;
    private GroundDetector groundDetector;
    public float inputX;
    private bool wasOnSlope;
    public bool isOnSlope;

    public void Initialize(GroundDetector gd)
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        groundDetector = gd;
    }
    public Vector3 newVelocity;

    public void HandleInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
    }

    public void ProcessMovement(bool isGrounded, RaycastHit groundHit, float dt)
    {
        if (inputX == 0 && isGrounded && rb.velocity.x == 0)
        {
            rb.velocity = new Vector3(0f, 0, 0f);
        }
        else
        {

        }

        float targetSpeed = (Input.GetKey(KeyCode.LeftShift) ? maxRunSpeed : maxWalkSpeed);
        float targetVx = inputX * targetSpeed;
        float accel = isGrounded ? targetSpeed / accelerationTime : (targetSpeed / accelerationTime) * airControl;
        float decel = isGrounded ? targetSpeed / decelerationTime : (targetSpeed / decelerationTime) * airControl;
        float newVx = Mathf.Abs(inputX) > 0.01f ? Mathf.MoveTowards(rb.velocity.x, targetVx, accel * dt) : Mathf.MoveTowards(rb.velocity.x, 0f, decel * dt);

        isOnSlope = IsOnSlope(groundHit) && isGrounded;

        if (isOnSlope)
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.right, groundHit.normal).normalized;
            float multi = 1f / slopeDir.x;
            newVelocity = slopeDir * newVx * multi;
            //rb.velocity = newVelocity;
        }
        else
        {
            newVelocity = new Vector3(newVx, isGrounded ? 0f : rb.velocity.y, 0f);

        }

        if (Mathf.Abs(inputX) > 0.01f)
        {
            Vector3 dir = new Vector3(inputX, 0, 0);
            Quaternion targetRot = Quaternion.LookRotation(dir);
            animator.transform.rotation = Quaternion.Slerp(
                animator.transform.rotation, targetRot, rotationSpeed * dt);
        }

        wasOnSlope = isOnSlope;
        rb.velocity = newVelocity;
    }

    public void UpdateAnimationStates()
    {
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        bool moving = Mathf.Abs(inputX) > 0.01f;
        animator.SetBool("Move", moving);
        animator.SetFloat("Speed", horizontalSpeed);

        animator.SetBool("Idle", !moving);
    }

    private bool IsOnSlope(RaycastHit hit)
    {
        float angle = Vector3.Angle(Vector3.up, hit.normal);
        return angle != 0f && angle < 55f;
    }


    public void ForceStop()
    {
        rb.velocity = Vector3.zero;
        UpdateAnimationStates();
    }
}
