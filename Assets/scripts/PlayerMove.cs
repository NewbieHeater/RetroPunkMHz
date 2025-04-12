using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    int count = 0;
    bool isJump;
    Vector3 moveVec;

    Rigidbody rigid;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();   
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        GetInput();
        Move();
        Turn();
        Jump();


    }
    
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
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

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            count = 0;
            anim.SetBool("isJump", false);
        }
    }
}
