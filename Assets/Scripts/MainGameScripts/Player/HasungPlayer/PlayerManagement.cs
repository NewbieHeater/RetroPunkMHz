using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerManagement : MonoBehaviour
{
    // Sub-Components
    public MovementController movementController;
    public JumpController jumpController;

    private CharacterController cc;
    public bool IsEnabled = true;
    public bool IsGrounded = false;
    void Awake()
    {
        cc = GetComponent<CharacterController>();

        movementController = new MovementController(cc);
        jumpController = new JumpController(cc);

        // �ʱ�ȭ: maxJumpHeight, timeToJumpApex ���� �ν����Ϳ��� �����ϼ���.
        jumpController.Initialize();
        movementController.Initialize();
        
    }

    void Update()
    {
        if (!IsEnabled) return;
        IsGrounded = cc.isGrounded;
        // �Է� ó��
        jumpController.HandleInput();
        movementController.HandleInput();

        // �ִϸ����� ������ �ʿ��� ���
        movementController.UpdateAnimator();
        jumpController.UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!IsEnabled) return;

        bool isGrounded = cc.isGrounded;
        // ����/�߷� �� vy
        float vY = jumpController.ProcessJump(isGrounded, Time.fixedDeltaTime);
        // �¿� �̵� �� vx
        float vX = movementController.ProcessMovement(isGrounded, Time.fixedDeltaTime);

        Vector3 move = new Vector3(vX, vY, 0f);
        cc.Move(move * Time.fixedDeltaTime);

        if ((cc.collisionFlags & CollisionFlags.Above) != 0 && vY > 0f)
        {
            jumpController.OnCeilingHit();
        }
    }

    public void SetAblePlayer(bool set)
    {
        IsEnabled = set;
        movementController.Reset();
        jumpController.Reset();
    }
}