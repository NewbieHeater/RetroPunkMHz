using UnityEngine;

public class PlayerBodyHeadLook : MonoBehaviour
{
    Camera cam;
    Animator animator;

    [Header("회전 설정")]
    [Tooltip("몸통 회전 속도 (°/초)")]
    public float bodyRotationSpeed = 180f;
    [Tooltip("머리 회전 속도 (°/초)")]
    public float headRotationSpeed = 360f;

    [Tooltip("플레이어(몸통) 기준 얼마나 떨어져 있어야 회전 시작할지")]
    public float lookRange = 1f;

    [Header("머리 IK 설정 (선택)")]
    [Range(0f, 1f)] public float lookWeight = 1f;
    public float lookClamp = 0.5f;
    // IK 대신 직접 본 회전을 원하면 headBone을 사용
    public Transform headBone;

    void Start()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();

        if (headBone == null && animator != null)
        {
            // Animator가 있고, Humanoid 애니메이션이면 Head 본 가져오기
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }

    private Vector3 GetAttackDirection()
    {
        // 1) 마우스 화면 좌표 → 월드 좌표
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 world = cam.ScreenToWorldPoint(mp);

        // 2) 자신의 위치(몸통)에서 마우스 위치까지 방향 벡터 계산
        Vector3 dir = world - transform.position;
        dir.z = 0f;  // 2D 플랫포머용 (Z 무시)
        return dir.normalized;
    }

    void Update()
    {
        // 1) 마우스 월드 좌표
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 worldPoint = cam.ScreenToWorldPoint(mp);

        // 2) 몸통(플레이어)과 마우스 사이 거리(XY 평면)
        Vector3 toMouse = worldPoint - transform.position;
        toMouse.z = 0f;
        float dist = toMouse.magnitude;

        if (dist <= lookRange)
        {
            // === 몸통 회전 ===
            // 3) 몸통이 바라볼 절대 각도 계산
            float targetBodyAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;
            float currentBodyAngle = transform.eulerAngles.z;
            if (currentBodyAngle > 180f) currentBodyAngle -= 360f;

            // 4) 최단 경로(±) 회전량 계산
            float bodyDelta = Mathf.DeltaAngle(currentBodyAngle, targetBodyAngle);
            float bodyMaxStep = bodyRotationSpeed * Time.deltaTime;
            float bodyStep = Mathf.Clamp(bodyDelta, -bodyMaxStep, bodyMaxStep);

            // 5) 실제 몸통 Z 회전 적용
            float newBodyZ = currentBodyAngle + bodyStep;
            transform.eulerAngles = new Vector3(0f, 0f, newBodyZ);

            // === 머리 회전 ===
            if (headBone != null)
            {
                // 6) 머리가 받아야 할 전체 각도는 body 회전 각(newBodyZ) + 머리가 조금 더 틀어질 상대 각도
                float targetHeadAngle = targetBodyAngle;
                float currentHeadLocalZ = headBone.localEulerAngles.z;
                if (currentHeadLocalZ > 180f) currentHeadLocalZ -= 360f;

                // 현재 머리가 바라보는 절대 각 = body’s newBodyZ + currentHeadLocalZ
                float currentHeadAbsoluteZ = newBodyZ + currentHeadLocalZ;
                // 머리가 새로 바라볼 절대 각 targetBodyAngle
                float headAbsDelta = Mathf.DeltaAngle(currentHeadAbsoluteZ, targetHeadAngle);

                // 머리 회전량은 headRotationSpeed * deltaTime
                float headMaxStep = headRotationSpeed * Time.deltaTime;
                float headStep = Mathf.Clamp(headAbsDelta, -headMaxStep, headMaxStep);

                // 새 절대 머리 각도
                float newHeadAbsoluteZ = currentHeadAbsoluteZ + headStep;
                // 이걸 로컬 회전으로 변환: localZ = newAbsoluteZ - bodyWorldZ
                float newHeadLocalZ = newHeadAbsoluteZ - newBodyZ;

                headBone.localEulerAngles = new Vector3(0f, 0f, newHeadLocalZ);
            }
        }
        else
        {
            // 범위 벗어나면 회전을 멈추고 원위치(또는 기본 포즈)로 되돌리려면 아래 로직 추가
            // 예) 몸통 회전 원복: transform.rotation = Quaternion.RotateTowards(...);
            // 머리도 localEulerAngles = Vector3.zero 등 기본값으로 보간한다면 자연스럽게 돌아갑니다.
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 위 코드에서 headBone 회전을 직접 제어하므로, IK LookAt은 꺼두거나 weight를 0으로 설정합니다.
        animator.SetLookAtWeight(0f);
    }
}
