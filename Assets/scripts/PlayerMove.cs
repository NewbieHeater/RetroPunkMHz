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
    private bool wDown;
    private bool jDown;
    private int count = 0;
    private bool isJump;
    private bool fDown;
    private bool fUp;
    private bool isCharging;
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

    }
    
    void GetInput()
    {
        //이름 바꿔줘요 hAxis제외하고
        hAxis = Input.GetAxisRaw("Horizontal");
        //z축 안써요 
        vAxis = Input.GetAxisRaw("Vertical");

        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButtonDown("Fire1");
        //mouseDown
        fUp = Input.GetButtonUp("Fire1");
    }
    private float onDashing = 0.3f;
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // z축을 안씀 
        //찾아보기
        //rigid.AddForce(moveVec, ForceMode.Impulse);

        //rigid.velocity = moveVec;

        transform.position += moveVec * speed * onDashing * Time.deltaTime;

        //위 3가지 방법중 적당한 방법 찾아쓰세요

        if (wDown)
        {
            transform.position += moveVec * speed * Time.deltaTime;
        }
        else
        {
            transform.position += moveVec * speed * onDashing * Time.deltaTime;
        }

        anim.SetBool("isRun", wDown);

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
        if (jDown && count < 2)
        {
            //AddForce쓰기
            rigid.velocity = new Vector3(0, 15, 0); 
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            count++;
        }
        

    }
    private float maxChargeTime;
    v거
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

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            count = 0;
            anim.SetBool("isJump", false);
        }
    }

    //Raycast나 콜라이더를 하나 추가해보세요
}
