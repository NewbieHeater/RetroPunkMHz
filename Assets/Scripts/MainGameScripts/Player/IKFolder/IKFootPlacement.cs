using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    Animator animator;
    

    [Header("발 IK 설정")]
    public LayerMask layer;
    [Range(0f, 1f)] public float DistanceToGround;
    [Range(0f, 2f)] public float footRayExtraHeight;
    public float footSpacing = 1f;

    

    // 내부 상태
    private Quaternion defaultLeftToeLocalRot, defaultRightToeLocalRot;
    private float hipOffset, goal, vel;
    private Vector3 footPos;

    void Start()
    {
        
        animator = GetComponent<Animator>();

        // 토우 기본 회전값 저장
        defaultLeftToeLocalRot = animator.GetBoneTransform(HumanBodyBones.LeftToes).localRotation;
        defaultRightToeLocalRot = animator.GetBoneTransform(HumanBodyBones.RightToes).localRotation;

        // 머리 본도 추출해두면 직접 제어 가능 (필요 시)
        

        // 힙 오프셋 초기화
        hipOffset = transform.localPosition.y;
        goal = hipOffset;
        vel = 1f;

        // 초기 왼발 위치
        footPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
    }

    void Update()
    {
        // 힙 높이 조절 (기존 로직 유지)
        if (Input.GetKeyDown(KeyCode.K)) goal = hipOffset - 0.1f;
        if (Input.GetKeyDown(KeyCode.L)) goal = hipOffset + 0.1f;
        hipOffset = Mathf.SmoothDamp(hipOffset, goal, ref vel, 0.35f, Mathf.Infinity);
        transform.localPosition = Vector3.up * hipOffset;
        
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
    float yL = 0f, yR = 0f;
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        RaycastHit hit;

        // —— LEFT FOOT IK ——
        Vector3 leftFootIKPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * footRayExtraHeight;
        if (Physics.Raycast(leftFootIKPos, Vector3.down, out hit, DistanceToGround + footRayExtraHeight, layer)
            && hit.transform.CompareTag("Ground"))
        {
            // 1) 위치 세팅 (기존 로직)
            Vector3 pos = hit.point;
            pos.y += DistanceToGround;
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, pos);

            // 2) 발 뼈대 원래 회전 & forward 가져오기
            Transform leftFootBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Quaternion originalRot = leftFootBone.rotation;
            Vector3 originalForward = leftFootBone.forward;

            // 3) 원래 forward 를 경사면 평면에 투영
            Vector3 slopeForward = Vector3.ProjectOnPlane(-originalForward, hit.normal).normalized;

            // 4) 투영된 forward 와 hit.normal 로 회전 만들기
            Quaternion slopeRot = Quaternion.LookRotation(slopeForward, hit.normal);

            // 5) “경사 틸트”만 적용하고 싶으면 slopeRot 을 그대로, 
            //    “원래 회전 보존 + 틸트”를 원하면 아래처럼 곱해줍니다:
            // Quaternion finalRot = slopeRot * Quaternion.Inverse(
            //     Quaternion.LookRotation(leftFootBone.forward, Vector3.up)
            // ) * originalRot;
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, pos);
            // 여기서는 전자(발이 slopeRot 그대로)를 사용
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, slopeRot);

            // 토우 회전은 초기값으로 복원
            animator.SetBoneLocalRotation(HumanBodyBones.LeftToes, defaultLeftToeLocalRot);
        }

        // —— RIGHT FOOT IK ——
        Vector3 rightFootIKPos = animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * footRayExtraHeight;
        if (Physics.Raycast(rightFootIKPos, Vector3.down, out hit, DistanceToGround + footRayExtraHeight, layer)
            && hit.transform.CompareTag("Ground"))
        {
            Vector3 pos = hit.point;
            pos.y += DistanceToGround;
            animator.SetIKPosition(AvatarIKGoal.RightFoot, pos);

            Transform rightFootBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Quaternion originalRot = rightFootBone.rotation;
            Vector3 originalForward = rightFootBone.forward;

            Vector3 slopeForward = Vector3.ProjectOnPlane(-originalForward, hit.normal).normalized;
            Quaternion slopeRot = Quaternion.LookRotation(slopeForward, hit.normal);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, pos);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, slopeRot);
            animator.SetBoneLocalRotation(HumanBodyBones.RightToes, defaultRightToeLocalRot);
        }
    }

    // —— 머리 LookAt IK 설정 —— 
    //animator.SetLookAtWeight(lookWeight, 0f, 1f, 1f, lookClamp);
    //animator.SetLookAtPosition(target.position);
}