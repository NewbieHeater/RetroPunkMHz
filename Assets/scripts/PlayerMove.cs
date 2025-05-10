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
    float hAxis;
    float vAxis;
    float chargeTime = 0;
    float chargeminTime = 1f;
    bool wDown;
    bool jDown;
    int count = 0;
    bool isJump;
    bool fDown;
    bool fUp;
    bool isCharging;
    Vector3 moveVec;

    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    weapon equipWeapon;
    void Awake()
    {
        
    }
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        equipWeapon = GameObject.Find("hand").GetComponent<weapon>();
    }

    // Update is called once per frame
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
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButtonDown("Fire1");
        fUp = Input.GetButtonUp("Fire1");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; // y√‡¿ª æ»æ∏

        transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        if (wDown)
        {
            transform.position += moveVec * speed * Time.deltaTime;
        }
        else
        {
            transform.position += moveVec * speed * 0.3f * Time.deltaTime;
        }
        anim.SetBool("isRun", wDown);
        anim.SetBool("isWalk", moveVec != Vector3.zero);
    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if (jDown && count < 2)
        {

            rigid.velocity = new Vector3(0, 15, 0); 
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            count++;
        }
        

    }
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
                if(chargeTime >5f)
                {
                    chargeTime = 5f;
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
}
