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

    //���� ������ �ٿ��α�(�Լ�����)
    //�ִ��� private�� ����!!!
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

    //�̰Ŵ� public�� �ƴϿ��� �ν�����â���� �ٲܼ��ְ�(�ٸ� ��ũ��Ʈ������ ���ٲ����� �ν����Ϳ����� �ٲ��)
    [SerializeField]
    private weapon equipWeapon;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        //Find����� ������ �ϴ� �̰� �ϳ������� �������ϴ�.
        equipWeapon = GameObject.Find("hand").GetComponent<weapon>();
    }

    void Update()
    {
        //���ƿ� �̰� ĸ��ȭ
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        FixedUpdate();
    }
    
    void GetInput()
    {
        //�̸� �ٲ���� hAxis�����ϰ�
        hAxis = Input.GetAxisRaw("Horizontal");
        //z�� �Ƚ�� 
        //vAxis = Input.GetAxisRaw("Vertical");
    
        JumpDown = Input.GetButtonDown("Jump");
        mouseDown = Input.GetButtonDown("Fire1");
        //mouseDown
        mouseUp = Input.GetButtonUp("Fire1");
    }
    
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, 0).normalized; // z���� �Ⱦ� 
        //ã�ƺ���
        //rigid.AddForce(moveVec, ForceMode.Impulse);

        //rigid.velocity = moveVec;
    
        
        rigid.velocity = moveVec * speed * Time.deltaTime;
    
       

        anim.SetBool("isRun", WalkDown);

        //���ǹ��ȿ� �־��ּ��� �׳� �ᵵ ��� ���µ� ������
        anim.SetBool("isWalk", moveVec != Vector3.zero);
    }
    void Turn()
    {
        //LookAt���� ���� ���콺������ ���� �����ֱ�
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
    private float maxChargeTime = 5f;
    void Attack()
    {
        if(mouseDown)
        {
            isCharging = true;
            chargeTime = 0f;
        }
        if(isCharging)
        {
            chargeTime += Time.deltaTime;
        }
        if(mouseUp)
        {
            if(chargeTime >= chargeminTime)
            {
                if(chargeTime > maxChargeTime)
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
        isFloor = Physics.Raycast(transform.position, Vector3.down, 0.1f, LayerMask.GetMask("Ground"));

        if (isFloor)
        {
            count= 0;
            anim.SetBool("isJump", false);
        }
    }

    
}
