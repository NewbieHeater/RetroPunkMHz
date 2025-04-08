using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int moveSpeed;
    [SerializeField] private int jumpPower;
    [SerializeField] private float rotationSpeed;

    private float lastHor = 1;
    public Vector3 movement;
    public bool isJumping = false;
    public bool isGrounded = true;
    private bool isSliding = false;
    private bool desiredJump, desiredSlide, desiredDash = false;

    public LayerMask groundLayerMask;

    void Start()
    {

    }

    void Update()
    {
        Move();
        CheckGrounded();

        if (Input.GetKeyDown("Jump"))
        {
            desiredJump = true;
        }

        if (desiredJump)
        {
            desiredJump = false;
            if (!isSliding)
                Jump();
        }
        if (desiredSlide)
        {
            desiredSlide = false;
            if (isGrounded)
            {

            }
            //Slide();
        }
    }
    public void Jump()
    {

    }
    public void Move()
    {
        float hor = Input.GetAxisRaw("Horizontal");

        // hor ���� 0�� �ƴ϶�� ������ �Է� ������ ����
        if (hor != 0)
            lastHor = hor;

        movement = new Vector3(hor, 0, 0).normalized;

        //animator.SetBool("Move", movement != Vector3.zero);

        transform.position += movement * moveSpeed * Time.deltaTime;

        // hor�� 0�̸� lastHor�� ����� ȸ�� ���� ����
        float faceDir = (hor == 0 ? lastHor : hor);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * faceDir), rotationSpeed);
    }

    void CheckGrounded()
    {
        isGrounded = isJumping ? false : (Physics.Raycast(transform.position + (transform.up * .05f), Vector3.down, .6f, groundLayerMask));
    }
}
