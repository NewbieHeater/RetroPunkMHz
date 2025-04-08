using UnityEngine;
using UnityEngine.TextCore.Text;

public class IdleState : IState<Character>
{
    private Character character;
    public void OperateEnter(Character sender)
    {
        character = sender;
    }
    public void OperateUpdate(Character sender)
    {

    }
    public void OperateFixedUpdate(Character sender)
    {

    }
    public void OperateExit(Character sender)
    {

    }

    public void HandleInput(Character sender)
    {
        // 점프 입력 시 JumpState로 전환
        if (Input.GetKeyDown(KeyCode.Space) && sender.isGrounded)
        {
            sender.sm.SetState(sender.dicState["Jump"]);
        }
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            sender.sm.SetState(sender.dicState["Move"]);
        }
        
    }
}