using Unity.Burst.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxWalkSpeed = 5f;
    public float maxRunSpeed = 5f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.2f;
    [Range(0f, 1f)] public float airControl = 0.5f;
    public float rotationSpeed = 0.2f;

    private Rigidbody rb;
    private Animator animator;
    private GroundDetector groundDetector;
    private float inputX;
    private bool wasOnSlope;
    public bool isOnSlope;

    public void Initialize(GroundDetector gd)
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        groundDetector = gd;
    }
    Vector3 newVelocity;
    [SerializeField] private bool shift;
    public void ProcessMovement(bool isGrounded, RaycastHit groundHit)
    {
        shift = Input.GetKey(KeyCode.LeftShift);
        inputX = Input.GetAxisRaw("Horizontal");
        float dt = Time.fixedDeltaTime;
        float speed = shift ? maxRunSpeed : maxWalkSpeed;
        float targetVx = inputX * speed;
        float accel = isGrounded ? speed / accelerationTime : (speed / accelerationTime) * airControl;
        float decel = isGrounded ? speed / decelerationTime : (speed / decelerationTime) * airControl;
        float newVx = Mathf.Abs(inputX) > 0.01f ? Mathf.MoveTowards(rb.velocity.x, targetVx, accel * dt) : Mathf.MoveTowards(rb.velocity.x, 0f, decel * dt);

        isOnSlope = IsOnSlope(groundHit);
        
        if (isOnSlope)
        {
            Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.right, groundHit.normal).normalized;
            Debug.Log(slopeDir);
            newVelocity = slopeDir * newVx;
            //rb.velocity = newVelocity;
        }
        else
        {
            //if (wasOnSlope)
            //    rb.velocity = new Vector3(newVx, -5f, 0f);
            //else
            //    newVelocity = new Vector3(newVx, rb.velocity.y, 0f);
            newVelocity = new Vector3(newVx, rb.velocity.y, 0f);
        }

        //if (!isOnSlope)
        //    newVelocity = new Vector3(newVx, rb.velocity.y, 0f);

        wasOnSlope = isOnSlope;
        rb.velocity = newVelocity;
    }

    public void UpdateAnimationStates()
    {
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        bool moving = Mathf.Abs(inputX) > 0.01f;
        animator.SetBool("Move", moving);
        animator.SetFloat("Speed", horizontalSpeed);

        if (moving)
            RotateCharacter();
        else
            animator.SetBool("Idle", true);
    }

    private bool IsOnSlope(RaycastHit hit)
    {
        float angle = Vector3.Angle(Vector3.up, hit.normal);
        return angle != 0f && angle < 55f;
    }

    private void RotateCharacter()
    {
        Vector3 direction = inputX > 0 ? Vector3.right : Vector3.left;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        Transform mesh = transform.GetChild(0);
        mesh.rotation = Quaternion.Slerp(mesh.rotation, targetRot, rotationSpeed);
        animator.SetBool("Idle", false);
    }
}