using UnityEngine;

public class Movement3D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;
    [SerializeField]
    private float jumpForce = 3.0f;
    private float gravity = -9.81f;
    private Vector3 moveDirection;
    public float rotSpeed = 0.2f;

    private CharacterController characterController;

    private Vector3 dir = Vector3.zero;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private float curTime;
    public float coolTime = 0.5f;
    public Transform pos;
    public Vector3 boxSize;

    private void Update()
    {
        dir.x = Input.GetAxis("Horizontal");
        
        if (characterController.isGrounded == false)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * dir.x), rotSpeed);
        }

        if (curTime <= 0)
        {
            //공격
            if (Input.GetKey(KeyCode.Z))
            {
                Collider[] colliders = Physics.OverlapBox(pos.position, boxSize);
                foreach (Collider collider in colliders)
                {
                    if (collider.tag == "Enemy")
                    {
                        //collider.GetComponent<Enemy>().TakeDamage(1);
                        Enemy enemy = collider.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            Vector3 attackDir = (collider.transform.position - transform.position).normalized; // 플레이어 → 적 방향

                            string attackType = "Normal";   // 공격 타입: "Normal"로 임시
                            int chargeLevel = 1;            // 일반 공격이니까 chargeLevel=1
                            enemy.LastAttack(attackType, chargeLevel, attackDir);
                        }
                        

                    }
                }
                curTime = coolTime;
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }

    }
    public void MoveTo(Vector3 direction)
    {
        moveDirection = new Vector3(direction.x, moveDirection.y, moveDirection.z);
    }

    public void JumpTo()
    {
        if (characterController.isGrounded == true)
        {
            moveDirection.y = jumpForce;
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }



    //private void FixedUpdate()
    //{
    //    if (dir != Vector3.zero)
    //    {
    //        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * dir.x), rotSpeed);
    //    }
    //}
}