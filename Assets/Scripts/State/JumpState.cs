using UnityEngine;

public class JumpState : IState<Character>
{
    private Character character;
    public void OperateEnter(Character sender)
    {
        character = sender;
        character.Jump();
        //character.animator.SetBool("Jump", true);
    }
    public void OperateExit(Character sender)
    {
        //character.animator.SetBool("Jump", false);
    }
    public void OperateUpdate(Character sender)
    {
        
    }
    public void OperateFixedUpdate(Character sender)
    {
        // ���� �߿��� ���� �̵��� Move()�� ó��
        character.Move();
    }
    public void HandleInput(Character sender)
    {
        if(sender.isGrounded)
        {
            sender.sm.SetState(sender.dicState["Idle"]);
        }
    }
}
