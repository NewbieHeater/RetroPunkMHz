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

        // 초기화: maxJumpHeight, timeToJumpApex 등은 인스펙터에서 세팅하세요.
        jumpController.Initialize();
        movementController.Initialize();
        
    }

    void Update()
    {
        if (!IsEnabled) return;
        IsGrounded = cc.isGrounded;
        // 입력 처리
        jumpController.HandleInput();
        movementController.HandleInput();

        // 애니메이터 갱신이 필요한 경우
        movementController.UpdateAnimator();
        jumpController.UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!IsEnabled) return;

        bool isGrounded = cc.isGrounded;
        // 점프/중력 → vy
        float vY = jumpController.ProcessJump(isGrounded, Time.fixedDeltaTime);
        // 좌우 이동 → vx
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