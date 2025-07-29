using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineCameraController : MonoBehaviour
{
    [Header("Ÿ��")]
    public Transform target;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("ī�޶� ����")]
    public float leadDistance = 2f;
    public float fallYOffset = 1f;
    public bool ignoreJump = true;
    public float groundCheckRadius = 0.2f;

    [Header("�ӵ� ��� Damping")]
    public float walkSmooth = 0.3f;
    public float runSmooth = 0.1f;
    public float maxWalkSpeed = 3f;
    public float maxRunSpeed = 6f;

    public GameObject Player;

    private CinemachineVirtualCamera vcam;
    private PlayerManagement playerMgmt;
    private CinemachineFramingTransposer transposer;
    private Rigidbody rb;
    private MovementController movementCtrl;

    private float groundY;
    private bool wasGrounded;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        playerMgmt = Player.GetComponent<PlayerManagement>();
        transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (target == null)
            target = vcam.Follow;

        rb = Player.GetComponent<Rigidbody>();
        movementCtrl = Player.GetComponent<MovementController>();

        groundY = Player.transform.position.y;
        wasGrounded = true;
    }

    private void LateUpdate()
    {
        if (target == null || transposer == null) return;

        bool grounded = playerMgmt.IsGrounded;
        float inputX = movementCtrl.inputX;
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        float verticalSpeed = rb.velocity.y;

        //���� �Ÿ� ���� (�̵� ���⿡ ���� ī�޶� ����)
        float leadOffset = Mathf.Abs(inputX) > 0.1f ? Mathf.Sign(inputX) * leadDistance : 0f;
        transposer.m_TrackedObjectOffset.x = leadOffset;

        //���� ���� ��� + ���� offset ����
        float targetYOffset = 0f;

        if (ignoreJump)
        {
            if (!wasGrounded && grounded)
                groundY = Player.transform.position.y;
            
            if (grounded)
                groundY = Player.transform.position.y;
            
            else if (Player.transform.position.y < groundY)
                groundY = Player.transform.position.y;


        }
        else { targetYOffset = 0f; }

        if (!grounded && verticalSpeed < 0f)
            targetYOffset -= fallYOffset;

        transposer.m_TrackedObjectOffset.y = targetYOffset;

        //�ӵ� ��� Damping ����
        float t = Mathf.InverseLerp(maxWalkSpeed, maxRunSpeed, horizontalSpeed);
        float smooth = Mathf.Lerp(walkSmooth, runSmooth, t);
        transposer.m_XDamping = smooth;
        transposer.m_YDamping = smooth;

        wasGrounded = grounded;
    }

    void Update()
    {
        
    }
}
