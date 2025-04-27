using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("캐릭터의 최대 이동 속도")]
    public float maxSpeed = 5f;
    [Tooltip("지면에서 가속 시간: 최대속도까지 도달하는 시간")]
    public float accelerationTime = 0.1f;
    [Tooltip("지면에서 감속 시간: 최대속도에서 정지까지 걸리는 시간")]
    public float decelerationTime = 0.2f;
    [Range(0f, 1f), Tooltip("공중 조작 정도 (0 = 공중 조작 불가, 1 = 지면 조작과 동일)")]
    public float airControl = 0.5f;
    [Tooltip("회전 속도")]
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
    /// jumpPhase에서 계산된 velocity.x를 포함한 벡터를 넘겨받아
    /// 최종 velocity.x만 업데이트합니다.
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
    /// FixedUpdate에서 Rigidbody에 적용할 최종 속도
    /// </summary>
    public Vector3 CurrentVelocity { get; private set; }

    public void ApplyVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }
}
