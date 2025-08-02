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
    public GameObject groundTarget;
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

    [Header("VCams")]
    public CinemachineVirtualCamera back;

    [Header("player")]
    public GameObject Player;

    private CinemachineVirtualCamera vcam;
    private RigidPlayerManagement playerMgmt;
    private CinemachineFramingTransposer transposer;
    private Rigidbody rb;
    private RigidMovementController movementCtrl;

    private float groundY;
    private bool wasGrounded;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        playerMgmt = Player.GetComponent<RigidPlayerManagement>();
        transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (target == null)
            target = vcam.Follow;

        rb = Player.GetComponent<Rigidbody>();
        movementCtrl = Player.GetComponent<RigidMovementController>();

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
        float leadOffset = Mathf.Abs(inputX) > 0.1f ? leadDistance : 0f;
        float leadOffsetTrue = inputX > 0 ? -leadOffset : leadOffset;
        transposer.m_TrackedObjectOffset.x = leadOffsetTrue;
        
        //���� ���� ���
        if (ignoreJump)
        {
            if (!wasGrounded && grounded)
                groundY = Player.transform.position.y;
            
            if (grounded)
                groundY = Player.transform.position.y;
            
            else if (Player.transform.position.y < groundY)
                groundY = Player.transform.position.y;
        }
        
        //�ӵ� ��� Damping ����
        float t = Mathf.InverseLerp(maxWalkSpeed, maxRunSpeed, horizontalSpeed);
        float smooth = Mathf.Lerp(walkSmooth, runSmooth, t);
        transposer.m_XDamping = smooth;
        transposer.m_YDamping = smooth;

        wasGrounded = grounded;
    }
    private void Update()
    {
        groundTarget.transform.position = new Vector3(groundTarget.transform.position.x, groundY, 0);
    }

    //�ߺ�ȣ�⶧���� priority �������� �ʰ� ����� �ٲ� 
    public void inVcamBack()
    {
        back.Priority = 15;
    }
    
    public void outVcamBack()
    {
        back.Priority = 9;
    }
}
