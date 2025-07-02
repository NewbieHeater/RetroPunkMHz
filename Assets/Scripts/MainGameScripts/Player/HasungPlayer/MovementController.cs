using Unity.Burst.CompilerServices;
using UnityEngine;

public class MovementController
{
    private CharacterController cc;
    public float inputX;
    private float velocityX;

    // 설정값
    public Transform model;
    public float maxWalkSpeed = 5f;
    public float maxRunSpeed = 8f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.1f;
    [Range(0f, 1f)] public float airControl = 1f;
    public float rotationSpeed = 10f;
    private Animator animator;

    public MovementController(CharacterController controller)
    {
        cc = controller;
        animator = controller.GetComponentInChildren<Animator>();
    }

    public void Initialize()
    {
        velocityX = 0f;
    }

    public void HandleInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
    }

    public float ProcessMovement(bool isGrounded, float dt)
    {
        float targetSpeed = (Input.GetKey(KeyCode.LeftShift) ? maxRunSpeed : maxWalkSpeed) * inputX;
        float accelRate = (isGrounded ? maxWalkSpeed / accelerationTime : maxWalkSpeed / accelerationTime * airControl);
        float decelRate = (isGrounded ? maxWalkSpeed / decelerationTime : maxWalkSpeed / decelerationTime * airControl);

        if (Mathf.Abs(inputX) > 0.01f)
            velocityX = Mathf.MoveTowards(velocityX, targetSpeed, accelRate * dt);
        else
            velocityX = Mathf.MoveTowards(velocityX, 0f, decelRate * dt);

        // 회전
        if (Mathf.Abs(inputX) > 0.01f)
        {
            Vector3 dir = new Vector3(inputX, 0, 0);
            Quaternion targetRot = Quaternion.LookRotation(dir);
            animator.transform.rotation = Quaternion.Slerp(
                animator.transform.rotation, targetRot, rotationSpeed * dt);
        }

        return velocityX;
    }

    public void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("Move", Mathf.Abs(inputX) > 0.01f);
        animator.SetFloat("Speed", Mathf.Abs(velocityX));
    }

    public void Reset()
    {
        velocityX = 0f;
    }
}