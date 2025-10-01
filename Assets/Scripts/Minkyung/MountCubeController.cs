using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MountCubeController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 7f;     // 플레이어보다 살짝 빠르게
    public float jumpForce = 5f;     // 점프 힘
    public float turnSpeed = 150f;   // 좌우 회전 속도

    private Rigidbody rb;
    private bool isGrounded;

    [HideInInspector] public bool canControl = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {

        if (!canControl) return;

        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // 전후 이동
        Vector3 move = transform.forward * v * moveSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // 좌우 회전
        transform.Rotate(Vector3.up * h * turnSpeed * Time.deltaTime);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // 간단한 바닥 체크
        if (col.contacts.Length > 0 && col.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }
}
