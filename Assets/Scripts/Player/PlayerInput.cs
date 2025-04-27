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
        movement.ProcessInput();            // 수평 입력 & 회전
        jump.ProcessInput();                // 점프 입력
        attack.ProcessInput();              // 공격 입력
        ground.CheckGround();               // 지면 판정
    }

    void FixedUpdate()
    {
        // 점프 물리 & 중력
        jump.HandlePhysics(ground.isGrounded);

        // 이동 처리 (점프에 의해 변형된 velocity.x 포함)
        movement.HandleMovement(ground.isGrounded, jump.CurrentVelocity);

        // 최종 속도 적용
        movement.ApplyVelocity(jump.CurrentVelocity);
    }
}
