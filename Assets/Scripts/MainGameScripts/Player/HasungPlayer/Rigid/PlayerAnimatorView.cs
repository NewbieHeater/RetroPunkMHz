// PlayerAnimatorView.cs
using UnityEngine;

public interface IPlayerAnimatorView
{
    void SetMove(bool moving, float speed);
    void SetGrounded(bool grounded);
    void SetFalling(bool falling);
    void TriggerJump();
    void Face(float xDir, float rotationSpeed, float dt);
}

public class PlayerAnimatorView : MonoBehaviour, IPlayerAnimatorView
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform model; // the thing you want to rotate (often animator.transform)

    private static readonly int HashMove = Animator.StringToHash("Move");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashGrounded = Animator.StringToHash("Grounded");
    private static readonly int HashFall = Animator.StringToHash("Fall");
    private static readonly int HashJump = Animator.StringToHash("JUMP");

    private void Reset()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!model && animator) model = animator.transform;
    }

    public void SetMove(bool moving, float speed)
    {
        if (!animator) return;
        animator.SetBool(HashMove, moving);
        animator.SetFloat(HashSpeed, speed);
    }

    public void SetGrounded(bool grounded)
    {
        if (!animator) return;
        animator.SetBool(HashGrounded, grounded);
    }

    public void SetFalling(bool falling)
    {
        if (!animator) return;
        animator.SetBool(HashFall, falling);
    }

    public void TriggerJump()
    {
        if (!animator) return;
        animator.SetTrigger(HashJump);
    }

    public void Face(float xDir, float rotationSpeed, float dt)
    {
        if (!model) return;
        if (Mathf.Abs(xDir) < 0.01f) return;

        Vector3 dir = new Vector3(Mathf.Sign(xDir), 0f, 0f);
        Quaternion target = Quaternion.LookRotation(dir);
        model.rotation = Quaternion.Slerp(model.rotation, target, rotationSpeed * dt);
    }
}
