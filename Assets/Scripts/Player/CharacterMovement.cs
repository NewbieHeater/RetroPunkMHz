using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("ĳ������ �ִ� �̵� �ӵ�")]
    public float maxSpeed = 5f;
    [Tooltip("���鿡�� ���� �ð�: �ִ�ӵ����� �����ϴ� �ð�")]
    public float accelerationTime = 0.1f;
    [Tooltip("���鿡�� ���� �ð�: �ִ�ӵ����� �������� �ɸ��� �ð�")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("���� ���� ���� (0 = ���� ���� �Ұ�, 1 = ���� ���۰� ����)")]
    public float airControl = 0.5f;
    [Tooltip("ȸ�� �ӵ�")]
    public float rotationSpeed = 0.2f;
    [SerializeField] private GameObject playerMesh;

    Rigidbody rb;
    float inputX;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (playerMesh == null)
            playerMesh = transform.GetChild(0).gameObject;
    }

    public void ProcessInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(inputX) > 0.01f)
            playerMesh.transform.rotation =
                Quaternion.Slerp(
                    playerMesh.transform.rotation,
                    Quaternion.LookRotation(Vector3.right * inputX),
                    rotationSpeed
                );
    }

    /// <summary>
    /// jumpPhase���� ���� velocity.x�� ������ ���͸� �Ѱܹ޾�
    /// ���� velocity.x�� ������Ʈ�մϴ�.
    /// </summary>
    public void HandleMovement(bool isGrounded, Vector3 fullVelocity)
    {
        float targetV = inputX * maxSpeed;
        float accel = maxSpeed / (isGrounded ? accelerationTime : (accelerationTime / airControl));
        float decel = maxSpeed / (isGrounded ? decelerationTime : (decelerationTime / airControl));

        float newX = Mathf.Abs(targetV) > 0.01f
            ? Mathf.MoveTowards(fullVelocity.x, targetV, accel * Time.fixedDeltaTime)
            : Mathf.MoveTowards(fullVelocity.x, 0f, decel * Time.fixedDeltaTime);

        fullVelocity.x = newX;
        CurrentVelocity = fullVelocity;
    }

    /// <summary>
    /// FixedUpdate���� Rigidbody�� ������ ���� �ӵ�
    /// </summary>
    public Vector3 CurrentVelocity { get; private set; }

    public void ApplyVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }
}
