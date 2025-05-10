using System.Xml.Serialization;
using UnityEngine;
using static UnityEditor.Progress;

public class Movement3D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;
    [SerializeField]
    private float jumpForce = 3.0f;
    private float gravity = -9.81f;
    private Vector3 moveDirection;
    [SerializeField]
    private float rotSpeed = 0.2f;

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

    public void Trun(float x)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * x), rotSpeed);
    }

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

        //gpt���� �����


        if (curTime <= 0)
        {
            //����
            if (Input.GetKey(KeyCode.Z))
            {
                Collider[] colliders = Physics.OverlapBox(pos.position, boxSize);
                foreach (Collider collider in colliders)
                {
                    //�±�  == �˻��� �±�   ��������
                    //compareTag����
                    if (collider.CompareTag("Enemy"))
                    {
                        //collider.GetComponent<Enemy>().TakeDamage(1);
                        Enemy enemy = collider.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            //���߿� �ǵ��

                            Vector3 attackDir = (collider.transform.position - transform.position).normalized; // �÷��̾� �� �� ����

                            string attackType = "Normal";   // ���� Ÿ��: "Normal"�� �ӽ�
                            int chargeLevel = 1;            // �Ϲ� �����̴ϱ� chargeLevel=1
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
    private int amplitued;
    private int amplitued1;
    private int amplitued2;
    private float chargeAttackDamage = 10f;
    float time;
    public void Attacked()
    {
        if(time <= 0)
        {
            time = 5f;

            //chargeAttackDamage / time;
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