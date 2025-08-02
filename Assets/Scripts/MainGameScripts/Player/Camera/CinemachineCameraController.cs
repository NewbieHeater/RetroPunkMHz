using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineCameraController : MonoBehaviour
{
    [Header("타겟")]
    public Transform target;
    public Transform groundCheck;
    public GameObject groundTarget;
    public LayerMask groundLayer;

    [Header("카메라 설정")]
    public float leadDistance = 2f;
    public float fallYOffset = 1f;
    public bool ignoreJump = true;
    public float groundCheckRadius = 0.2f;

    [Header("속도 기반 Damping")]
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

        //리드 거리 적용 (이동 방향에 따른 카메라 선행)
        float leadOffset = Mathf.Abs(inputX) > 0.1f ? leadDistance : 0f;
        float leadOffsetTrue = inputX > 0 ? -leadOffset : leadOffset;
        transposer.m_TrackedObjectOffset.x = leadOffsetTrue;
        
        //점프 무시 기능
        if (ignoreJump)
        {
            if (!wasGrounded && grounded)
                groundY = Player.transform.position.y;
            
            if (grounded)
                groundY = Player.transform.position.y;
            
            else if (Player.transform.position.y < groundY)
                groundY = Player.transform.position.y;
        }
        
        //속도 기반 Damping 조절
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

    //중복호출때문에 priority 증감하지 않고 상수로 바꿈 
    public void inVcamBack()
    {
        back.Priority = 15;
    }
    
    public void outVcamBack()
    {
        back.Priority = 9;
    }
}
