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
    private float chargePauseTime = 0.3f;
    private float temp = 1;
    private bool WalkDown;
    private bool JumpDown;
    private int count = 0;
    private bool isJump = false;
    private bool mouseDown;
    private bool mouseUp;
    private bool isCharging;
    private bool isFloor = false;
    private bool hasPaused = false;
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
        //moveAttack();
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
        moveVec = new Vector3(hAxis * speed, rigid.velocity.y, 0);
        if (mouseDown)
        {
            isCharging = true;
            chargeTime = 0f;
            if (moveVec == Vector3.zero)
            {
               
                anim.Play("chargeAttack", 0, 0f);
                StartCoroutine(PauseAnimationAfterDelay(chargePauseTime)); // 0.3초후 애니메이션 멈춤
            }
                
        }
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
        }
        if (mouseUp && isCharging)
        {

            isCharging = false;
            anim.speed = 1f;
            if (chargeTime >= chargeminTime)
            {
                if (chargeTime > maxChargeTime)
                {
                    
                    chargeTime = maxChargeTime;
                    equipWeapon.ChargeSwing(chargeTime);
                }
                else
                {
                    
                    equipWeapon.ChargeSwing(chargeTime);

                }
            }
            else
            {
                if(moveVec != Vector3.zero)
                {
                    
                    anim.SetTrigger("doAttack");
                    equipWeapon.Swing();
                }
                else
                {
                    anim.SetTrigger("doSwing");
                    equipWeapon.Swing();
                }
                    
                

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

    /*void moveAttack()
    {
        if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 1f)
        {
            if (temp >= 0)
            {
                temp -= Time.deltaTime;
            }
            anim.SetLayerWeight(1, 0);
        }
    }*/
    void FixedUpdate()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 dir = Vector3.down;
        float length = 0.8f;
        int mask = LayerMask.GetMask("Ground");

        // 실제 레이캐스트
        isFloor = Physics.Raycast(origin, dir, length, mask);

        // 디버그용 레이 그리기 (Scene 뷰에서만 보입니다)
        Debug.DrawRay(origin, dir * length, isFloor ? Color.green : Color.red);

        
        anim.SetFloat("velocityY", rigid.velocity.y);
        if (!isJump && isFloor)
        {
            
            anim.SetTrigger("Land");
            count = 0;
            
        }
        isJump = isFloor;
    }

    IEnumerator PauseAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isCharging)
        {
            anim.speed = 0f; // 애니메이션 멈춤
            hasPaused = true;
        }
    }

}


