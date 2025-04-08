using UnityEngine;
using UnityEngine.TextCore.Text;

public class MoveState : IState<Character>
{
    private Character character;
    public void OperateEnter(Character sender)
    {
        character = sender;
        //character.animator.SetBool("Move", true);
    }

    public void OperateExit(Character sender)
    {
        //character.animator.SetBool("Move", false);
    }

    public void OperateUpdate(Character sender)
    {
        
    }
    public void OperateFixedUpdate(Character sender)
    {
        character.Move();
    }
    public void HandleInput(Character sender)
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f)
        {
            sender.sm.SetState(sender.dicState["Idle"]);
        }
        if (Input.GetKeyDown(KeyCode.Space) && sender.isGrounded)
        {
            sender.sm.SetState(sender.dicState["Jump"]);
        }
    }
}