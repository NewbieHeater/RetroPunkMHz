using UnityEngine;

public class PlayerBodyHeadLook : MonoBehaviour
{
    Camera cam;
    Animator animator;

    [Header("ȸ�� ����")]
    [Tooltip("���� ȸ�� �ӵ� (��/��)")]
    public float bodyRotationSpeed = 180f;
    [Tooltip("�Ӹ� ȸ�� �ӵ� (��/��)")]
    public float headRotationSpeed = 360f;

    [Tooltip("�÷��̾�(����) ���� �󸶳� ������ �־�� ȸ�� ��������")]
    public float lookRange = 1f;

    [Header("�Ӹ� IK ���� (����)")]
    [Range(0f, 1f)] public float lookWeight = 1f;
    public float lookClamp = 0.5f;
    // IK ��� ���� �� ȸ���� ���ϸ� headBone�� ���
    public Transform headBone;

    void Start()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();

        if (headBone == null && animator != null)
        {
            // Animator�� �ְ�, Humanoid �ִϸ��̼��̸� Head �� ��������
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }

    private Vector3 GetAttackDirection()
    {
        // 1) ���콺 ȭ�� ��ǥ �� ���� ��ǥ
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 world = cam.ScreenToWorldPoint(mp);

        // 2) �ڽ��� ��ġ(����)���� ���콺 ��ġ���� ���� ���� ���
        Vector3 dir = world - transform.position;
        dir.z = 0f;  // 2D �÷����ӿ� (Z ����)
        return dir.normalized;
    }

    void Update()
    {
        // 1) ���콺 ���� ��ǥ
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 worldPoint = cam.ScreenToWorldPoint(mp);

        // 2) ����(�÷��̾�)�� ���콺 ���� �Ÿ�(XY ���)
        Vector3 toMouse = worldPoint - transform.position;
        toMouse.z = 0f;
        float dist = toMouse.magnitude;

        if (dist <= lookRange)
        {
            // === ���� ȸ�� ===
            // 3) ������ �ٶ� ���� ���� ���
            float targetBodyAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;
            float currentBodyAngle = transform.eulerAngles.z;
            if (currentBodyAngle > 180f) currentBodyAngle -= 360f;

            // 4) �ִ� ���(��) ȸ���� ���
            float bodyDelta = Mathf.DeltaAngle(currentBodyAngle, targetBodyAngle);
            float bodyMaxStep = bodyRotationSpeed * Time.deltaTime;
            float bodyStep = Mathf.Clamp(bodyDelta, -bodyMaxStep, bodyMaxStep);

            // 5) ���� ���� Z ȸ�� ����
            float newBodyZ = currentBodyAngle + bodyStep;
            transform.eulerAngles = new Vector3(0f, 0f, newBodyZ);

            // === �Ӹ� ȸ�� ===
            if (headBone != null)
            {
                // 6) �Ӹ��� �޾ƾ� �� ��ü ������ body ȸ�� ��(newBodyZ) + �Ӹ��� ���� �� Ʋ���� ��� ����
                float targetHeadAngle = targetBodyAngle;
                float currentHeadLocalZ = headBone.localEulerAngles.z;
                if (currentHeadLocalZ > 180f) currentHeadLocalZ -= 360f;

                // ���� �Ӹ��� �ٶ󺸴� ���� �� = body��s newBodyZ + currentHeadLocalZ
                float currentHeadAbsoluteZ = newBodyZ + currentHeadLocalZ;
                // �Ӹ��� ���� �ٶ� ���� �� targetBodyAngle
                float headAbsDelta = Mathf.DeltaAngle(currentHeadAbsoluteZ, targetHeadAngle);

                // �Ӹ� ȸ������ headRotationSpeed * deltaTime
                float headMaxStep = headRotationSpeed * Time.deltaTime;
                float headStep = Mathf.Clamp(headAbsDelta, -headMaxStep, headMaxStep);

                // �� ���� �Ӹ� ����
                float newHeadAbsoluteZ = currentHeadAbsoluteZ + headStep;
                // �̰� ���� ȸ������ ��ȯ: localZ = newAbsoluteZ - bodyWorldZ
                float newHeadLocalZ = newHeadAbsoluteZ - newBodyZ;

                headBone.localEulerAngles = new Vector3(0f, 0f, newHeadLocalZ);
            }
        }
        else
        {
            // ���� ����� ȸ���� ���߰� ����ġ(�Ǵ� �⺻ ����)�� �ǵ������� �Ʒ� ���� �߰�
            // ��) ���� ȸ�� ����: transform.rotation = Quaternion.RotateTowards(...);
            // �Ӹ��� localEulerAngles = Vector3.zero �� �⺻������ �����Ѵٸ� �ڿ������� ���ư��ϴ�.
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // �� �ڵ忡�� headBone ȸ���� ���� �����ϹǷ�, IK LookAt�� ���ΰų� weight�� 0���� �����մϴ�.
        animator.SetLookAtWeight(0f);
    }
}
