using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    Animator animator;
    Camera cam;

    [Header("발 IK 설정")]
    public LayerMask layer;
    [Range(0f, 1f)] public float DistanceToGround;
    [Range(0f, 2f)] public float footRayExtraHeight;
    public float footSpacing = 1f;

    [Header("머리 LookAt 설정")]
    public Transform target;           // 씬에 빈 GameObject 하나를 할당
    [Range(0f, 1f)] public float lookWeight = 1f;
    public float lookClamp = 0.5f; // 얼마나 엄격하게 머리를 고정할지
    Transform headBone;

    // 내부 상태
    private Quaternion defaultLeftToeLocalRot, defaultRightToeLocalRot;
    private float hipOffset, goal, vel;
    private Vector3 footPos;

    void Start()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();

        // 토우 기본 회전값 저장
        defaultLeftToeLocalRot = animator.GetBoneTransform(HumanBodyBones.LeftToes).localRotation;
        defaultRightToeLocalRot = animator.GetBoneTransform(HumanBodyBones.RightToes).localRotation;

        // 머리 본도 추출해두면 직접 제어 가능 (필요 시)
        headBone = animator.GetBoneTransform(HumanBodyBones.Head);

        // 힙 오프셋 초기화
        hipOffset = transform.localPosition.y;
        goal = hipOffset;
        vel = 1f;

        // 초기 왼발 위치
        footPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
    }

    private Vector3 GetAttackDirection()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 world = cam.ScreenToWorldPoint(mp);
        Vector3 dir = (world - transform.position);
        dir.z = 0f;
        return dir.normalized;
    }
    void Update()
    {
        // 힙 높이 조절 (기존 로직 유지)
        if (Input.GetKeyDown(KeyCode.K)) goal = hipOffset - 0.1f;
        if (Input.GetKeyDown(KeyCode.L)) goal = hipOffset + 0.1f;
        hipOffset = Mathf.SmoothDamp(hipOffset, goal, ref vel, 0.35f, Mathf.Infinity);
        transform.localPosition = Vector3.up * hipOffset;

        float headZ = headBone.position.z;

        // 2) ScreenPoint → Ray
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // 3) 머리 Z 고정 평면 정의 (normal = Vector3.forward, distance = headZ)
        Plane lookPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, headZ));

        // 4) Ray가 평면과 교차하면 그 지점으로 target 옮기기
        if (lookPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            // worldPoint 는 (X, Y, headZ) 형태가 나오게 됩니다.
            target.position = worldPoint;
        }
    }

    
    //Ray ray1 = new Ray(body.position + (body.right * footSpacing) + body.forward * 0.5f + (body.up), Vector3.down);
    //if (Physics.Raycast(ray1, out RaycastHit hit1, 10, layer))
    //{ 
    //    if(Mathf.Abs(hit1.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y) < 1)
    //    {
    //        hipOffset = Mathf.SmoothDamp(hipOffset, goal - Mathf.Abs(hit1.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y), ref vel, 0.35f, Mathf.Infinity);
    //        transform.localPosition = Vector3.up * hipOffset;
    //        Debug.Log("adsf");
    //    }
    //}
    //animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("LeftFootIKWeight"));
    //animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("LeftFootIKWeight"));
    //animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("RightFootIKWeight"));
    //animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("RightFootIKWeight"));
    //animator.SetIKRotation(AvatarIKGoal.)
    //if (Mathf.Abs(hit.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y) < 1)
    //{
    //    hipOffset = Mathf.SmoothDamp(hipOffset, goal + (hit.point.y - footPos.y), ref vel, 0.35f, Mathf.Infinity);
    //    transform.localPosition = Vector3.up * hipOffset;
    //    //Debug.Log(Mathf.Abs(hit.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y));
    //}
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // —— 머리 LookAt IK 설정 —— 
        //animator.SetLookAtWeight(lookWeight, 0f, 1f, 1f, lookClamp);
        //animator.SetLookAtPosition(target.position);

        // —— 발 IK 설정 —— 
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

        RaycastHit hit;
        // 왼발
        Ray rayL = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(rayL, out hit, DistanceToGround + footRayExtraHeight, layer)
            && hit.transform.CompareTag("Ground"))
        {
            Vector3 pos = hit.point;
            pos.y += DistanceToGround;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            Quaternion rot = Quaternion.LookRotation(forward, hit.normal);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, pos);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, rot);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftToes, defaultLeftToeLocalRot);
        }

        // 오른발
        Ray rayR = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(rayR, out hit, DistanceToGround + footRayExtraHeight, layer)
            && hit.transform.CompareTag("Ground"))
        {
            Vector3 pos = hit.point;
            pos.y += DistanceToGround;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            Quaternion rot = Quaternion.LookRotation(forward, hit.normal);

            animator.SetIKPosition(AvatarIKGoal.RightFoot, pos);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rot);
            animator.SetBoneLocalRotation(HumanBodyBones.RightToes, defaultRightToeLocalRot);
        }
    }
}