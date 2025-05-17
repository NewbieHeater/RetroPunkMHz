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
    private float maxChargeTime = 5f;
    private float hAxis;
    private float vAxis;
    private float chargeTime = 0;
    private float chargeminTime = 1f;
    private bool WalkDown;
    private bool JumpDown;
    private int count = 0;
    private bool isJump;
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
        anim = GetComponent<Animator>();


        equipWeapon = GameObject.Find("hand").GetComponent<weapon>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
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
        moveVec = new Vector3(hAxis, 0, 0).normalized;

        rigid.velocity = moveVec * speed * Time.deltaTime;

        anim.SetBool("isWalk", moveVec != Vector3.zero);
    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if (JumpDown && count < 2)
        {

            rigid.AddForce(new Vector3(0, 15, 0), ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            count++;
            JumpDown = false;
        }


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
                    chargeTime = maxChargeTime;
                    isCharging = false;
                    equipWeapon.ChargeSwing(chargeTime);
                }
                else
                {
                    isCharging = false;
                    equipWeapon.ChargeSwing(chargeTime);

                }
            }
            else
            {
                isCharging = false;
                equipWeapon.Swing();

            }
        }
    }

    void FixedUpdate()
    {
        isFloor = Physics.Raycast(transform.position + Vector3.up*0.5f, Vector3.down, 0.6f, LayerMask.GetMask("Ground"));

        if (isFloor)
        {
            count = 0;
            anim.SetBool("isJump", false);
        }
    }
}


