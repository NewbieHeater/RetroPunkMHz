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

    }
    
    void GetInput()
    {
        //�̸� �ٲ���� hAxis�����ϰ�
        hAxis = Input.GetAxisRaw("Horizontal");
        //z�� �Ƚ�� 
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
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // z���� �Ⱦ� 
        //ã�ƺ���
        //rigid.AddForce(moveVec, ForceMode.Impulse);

        //rigid.velocity = moveVec;

        transform.position += moveVec * speed * onDashing * Time.deltaTime;

        //�� 3���� ����� ������ ��� ã�ƾ�����

        if (wDown)
        {
            transform.position += moveVec * speed * Time.deltaTime;
        }
        else
        {
            transform.position += moveVec * speed * onDashing * Time.deltaTime;
        }

        anim.SetBool("isRun", wDown);

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
        if (jDown && count < 2)
        {
            //AddForce����
            rigid.velocity = new Vector3(0, 15, 0); 
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            count++;
        }
        

    }
    private float maxChargeTime;
    void Attack()
    {
        if(fDown)
        {
            isCharging = true;
            chargeTime = 0f;
        }
        if(isCharging)
        {
            chargeTime += Time.deltaTime;
        }
        if(fUp)
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

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            count = 0;
            anim.SetBool("isJump", false);
        }
    }

    //Raycast�� �ݶ��̴��� �ϳ� �߰��غ�����
}
