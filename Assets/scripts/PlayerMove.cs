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

    //접근 한정자 붙여두기(함수에도)
    //최대한 private를 쓰자!!!
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

    //이거는 public이 아니여도 인스펙터창에서 바꿀수있게(다른 스크립트에서는 못바꾸지만 인스펙터에서는 바뀌게)
    [SerializeField]
    private weapon equipWeapon;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        //Find사용을 줄이자 일단 이거 하나정도는 괜찮습니다.
        equipWeapon = GameObject.Find("hand").GetComponent<weapon>();
    }

    void Update()
    {
        //좋아요 이거 캡슐화
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        FixedUpdate();
    }
    
    void GetInput()
    {
        //이름 바꿔줘요 hAxis제외하고
        hAxis = Input.GetAxisRaw("Horizontal");
        //z축 안써요 
        //vAxis = Input.GetAxisRaw("Vertical");
    
        JumpDown = Input.GetButtonDown("Jump");
        mouseDown = Input.GetButtonDown("Fire1");
        //mouseDown
        mouseUp = Input.GetButtonUp("Fire1");
    }
    
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, 0).normalized; // z축을 안씀 
        //찾아보기
        //rigid.AddForce(moveVec, ForceMode.Impulse);

        //rigid.velocity = moveVec;
    
        
        rigid.velocity = moveVec * speed * Time.deltaTime;
    
       

        anim.SetBool("isRun", WalkDown);

        //조건문안에 넣어주세요 그냥 써도 상관 없는데 가독성
        anim.SetBool("isWalk", moveVec != Vector3.zero);
    }
    void Turn()
    {
        //LookAt말고 직접 마우스쪽으로 각도 돌려주기
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
