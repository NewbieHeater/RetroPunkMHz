using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    public float speed;
    public int Amlitude;
    public int Period;
    public int Waveform;
    public GameObject[] weapon;
    public Transform playerMesh;
    private float maxChargeTime = 5f;
    private float hAxis;
    private float vAxis;
    private float chargeTime = 0;
    private float chargeminTime = 1f;
    private bool WalkDown;
    private bool JumpDown;
    private int count = 0;
    private bool isJump = false;
    private bool mouseDown;
    private bool mouseUp;
    private bool isCharging;
    private bool isFloor = false;
    private Vector3 moveVec;

    private Rigidbody rigid;
    private Animator anim;

    private GameObject nearObject;
   

    [SerializeField]
    private weapon equipWeapon;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        equipWeapon = GameObject.Find("hand").GetComponent<weapon>();
    }

    void Update()
    {
        GetInput();
        Move();
        Jump();
        Attack();
        HandleRotation();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        JumpDown = Input.GetButtonDown("Jump");
        mouseDown = Input.GetButtonDown("Fire1");
        mouseUp = Input.GetButtonUp("Fire1");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis*speed, rigid.velocity.y, 0);

        rigid.velocity = moveVec;

        anim.SetBool("isRun", moveVec != Vector3.zero);
    }
    private void HandleRotation()
    {
        if (Mathf.Abs(rigid.velocity.x) > 0.01f)
            playerMesh.rotation = Quaternion.Slerp(playerMesh.rotation,
                Quaternion.LookRotation(Vector3.right * rigid.velocity.x), 0.1f);
    }

    
  


    void Attack()
    {
        if (mouseDown)
        {
            isCharging = true;
            chargeTime = 0f;
        }
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
        }
        if (mouseUp)
        {
            
            if (chargeTime >= chargeminTime)
            {
                if (chargeTime > maxChargeTime)
                {
                    anim.SetTrigger("doSwing");
                    chargeTime = maxChargeTime;
                    isCharging = false;
                    equipWeapon.ChargeSwing(chargeTime);
                }
                else
                {
                    anim.SetTrigger("doSwing");
                    isCharging = false;
                    equipWeapon.ChargeSwing(chargeTime);

                }
            }
            else
            {
                anim.SetTrigger("doSwing");
                isCharging = false;
                equipWeapon.Swing();
                

            }
        }
    }
    void Jump()
    {
        if (JumpDown && count < 2)
        {

            rigid.AddForce(Vector3.up * 20f, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            count++;
            JumpDown = false;
        }


    }

    void FixedUpdate()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = Vector3.down;
        float length = 0.8f;
        int mask = LayerMask.GetMask("Ground");

        // ���� ����ĳ��Ʈ
        isFloor = Physics.Raycast(origin, dir, length, mask);

        // ����׿� ���� �׸��� (Scene �信���� ���Դϴ�)
        Debug.DrawRay(origin, dir * length, isFloor ? Color.green : Color.red);

        
        anim.SetFloat("velocityY", rigid.velocity.y);
        if (!isJump && isFloor)
        {
            
            anim.SetTrigger("Land");
            count = 0;
            
        }
        isJump = isFloor;
    }
    
}


