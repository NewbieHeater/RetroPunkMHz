using UnityEngine;

[RequireComponent(typeof(CharacterMovement),typeof(CharacterJump), typeof(AttackController))]
public class PlayerInput : MonoBehaviour
{
    CharacterMovement movement;
    CharacterJump jump;
    GroundDetector ground;
    AttackController attack;

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        jump = GetComponent<CharacterJump>();
        ground = GetComponent<GroundDetector>();
        attack = GetComponent<AttackController>();
    }

    void Update()
    {
        movement.ProcessInput();            // ���� �Է� & ȸ��
        jump.ProcessInput();                // ���� �Է�
        attack.ProcessInput();              // ���� �Է�
        ground.CheckGround();               // ���� ����
    }

    void FixedUpdate()
    {
        // ���� ���� & �߷�
        jump.HandlePhysics(ground.isGrounded);

        // �̵� ó�� (������ ���� ������ velocity.x ����)
        movement.HandleMovement(ground.isGrounded, jump.CurrentVelocity);

        // ���� �ӵ� ����
        movement.ApplyVelocity(jump.CurrentVelocity);
    }
}
