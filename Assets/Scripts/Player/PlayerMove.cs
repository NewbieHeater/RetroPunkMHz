using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//using UnityEngine.Windows; 이거 때문에 오류 뜨는데 뭔지 모르겠음

public class PlayerMove : MonoBehaviour
{
    public float speed;
    public int Amlitude;
    public int Period;
    public int Waveform;
    public GameObject[] weapon;
    public Transform playerMesh;
    public int damage;
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
    private float attackCapsuleHeight = 1f;
    private float changedamage;
    private float attackTime = 0.3f;
    private Collider[] _overlapResults = new Collider[16];
    private Vector3 moveVec;

    private Rigidbody rigid;
    private Rigidbody rb;
    private Animator anim;
    private Vector3 velocity;
    private GameObject nearObject;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private float attackRange = 2f;
    

    /*public struct DamageInfo
    {
        public int Amount;          // 입힐 피해량
        public Vector3 SourceDir;     // 공격 원점→목표 방향
        public float KnockbackForce; // 넉백 세기 (차지 공격일 때만 의미)
    }*/
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetInput();
        Move();
        Jump();
        Attack();
        HandleRotation();
        GetLastInputDirection();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        JumpDown = Input.GetButtonDown("Jump");
        mouseDown = Input.GetButtonDown("Fire1");
        mouseUp = Input.GetButtonUp("Fire1");
        ProcessInput();
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


    public string ColorString(string s, string color)
    {
        string final = $"<color={color}>{s}</color>";
        string leftTag = "<color=" + color + ">";
        string rightTag = "</color>";

        return leftTag + s + rightTag;
    }



    void Attack()
    {
        moveVec = new Vector3(hAxis * speed, rigid.velocity.y, 0);
        if (mouseDown)
        {
            isCharging = true;
            chargeTime = 0f;
            anim.SetTrigger("docharge");
   
        }
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
        }
        if (mouseUp && isCharging)
        {

            isCharging = false;
            if (chargeTime >= chargeminTime)
            {
                
                if (chargeTime > maxChargeTime )
                {
                    
                    anim.SetTrigger("dochargeAttack");
                    chargeTime = maxChargeTime;
                    ChargeSwing(chargeTime);
                }
                else if(chargeTime <= maxChargeTime )
                {
                    
                    anim.SetTrigger("dochargeAttack");
                    ChargeSwing(chargeTime);

                }
            }
            else
            {
                anim.SetTrigger("doAttack");
                swing();


            }
        }
    }
    void Jump()
    {
        if (JumpDown && count < 2)
        {

            rigid.AddForce(Vector3.up * 20f, ForceMode.Impulse);
            
            anim.SetTrigger("doJump");
            count++;
            JumpDown = false;
        }


    }
    Vector3 lastMoveDir = Vector3.right;
    private void ProcessInput()
    {
        velocity.x = rb.velocity.x;
        if (Mathf.Abs(hAxis) > 0.001f)
        {
            lastMoveDir = new Vector3(hAxis, 0f, 0f).normalized;
        }
    }
    private Vector3 GetLastInputDirection()
    {
        // 만약 lastMoveDir이 벡터 (±1,0,0)이어서 공격의 XY 평면 방향으로 적절히 사용됩니다.
        // 그 외에도 필요 시 y축으로 변환한다면 여기에 로직 추가 가능
        Vector3 dir = lastMoveDir;
        dir.z = 0f;
        return dir.normalized;
    }
    

    void swing()
    {
        var info = new DamageInfo
        {
            Amount = damage,
            SourceDir = GetLastInputDirection(), // 또는 GetAttackDirection()
            KnockbackForce = 0
        };
        ExecuteAttack(info);
        Debug.Log("데미지: " + damage);
    }

    public void ChargeSwing(float chargePower)
    {
        changedamage = Mathf.Clamp(chargePower * damage, damage, 100);
        var info = new DamageInfo
        {
            Amount = (int)changedamage,
            SourceDir = GetLastInputDirection(), // 또는 GetAttackDirection()
            KnockbackForce = chargePower
        };
        ExecuteAttack(info);
        Debug.Log("차지 공격! 데미지: " + changedamage);
    }
    private void ExecuteAttack(in DamageInfo info)
    {
        Vector3 dir = GetLastInputDirection(); // lastMoveDir이 들어있음
        Vector3 tipCenter = transform.position + Vector3.up + dir * attackRange / 2;
        attackRadius = attackRange / 2;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f).normalized;
        float halfHeight = attackCapsuleHeight * 0.5f;

        Vector3 pointA = tipCenter + perp * halfHeight;
        Vector3 pointB = tipCenter - perp * halfHeight;

        int mask = LayerMask.GetMask("Enemy", "Destructible");
        int hitCount = Physics.OverlapCapsuleNonAlloc(
            pointA,
            pointB,
            attackRadius,
            _overlapResults,
            mask,
            QueryTriggerInteraction.Collide
        );
        Debug.Log($"OverlapCapsule hitCount = {hitCount}");
        for (int i = 0; i < hitCount; i++)
        {
            if (_overlapResults[i].TryGetComponent<IAttackable>(out var atk))
                atk.TakeDamage(info);
        }
    }
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
        isJump = isFloor; // 점프를 하면 isFloor이 false이기 때문에 isJump가 false로 바뀜
    }

    

}


