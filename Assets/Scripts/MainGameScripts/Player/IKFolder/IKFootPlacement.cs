using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(Animator))]
public class IKFootPlacement : MonoBehaviour
{
    [Header("발 IK 설정")]
    public LayerMask layer;
    [Range(0f, 1f)] public float DistanceToGround = 0.1f;
    [Range(0f, 8f)] public float footRayExtraHeight = 1f;
    [Range(0f, 1f)] public float hipSmoothingTime = 0.3f;

    [Header("절벽에서 한쪽발만 있을 때 골반 드롭")]
    [Range(0f, 0.5f)] public float cliffDropAmount = 0.15f;

    private Animator animator;
    private Quaternion defaultLeftToeLocalRot, defaultRightToeLocalRot;
    private float baseHipHeight, hipOffset, hipVelocity, goalHipHeight;

    void Start()
    {
        animator = GetComponent<Animator>();

        defaultLeftToeLocalRot = animator.GetBoneTransform(HumanBodyBones.LeftToes).localRotation;
        defaultRightToeLocalRot = animator.GetBoneTransform(HumanBodyBones.RightToes).localRotation;

        baseHipHeight = transform.localPosition.y;
        hipOffset = baseHipHeight;
        goalHipHeight = baseHipHeight;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
    }

    void Update()
    {
        // goalHipHeight로 부드럽게 이동
        hipOffset = Mathf.SmoothDamp(hipOffset, goalHipHeight, ref hipVelocity, hipSmoothingTime);
        transform.localPosition = new Vector3(0f, hipOffset, 0f);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 항상 IK 가중치 100%
        

        // LEFT FOOT
        RaycastHit hit;
        Vector3 leftOrigin = animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 1;
        bool leftHit = Physics.Raycast(
            leftOrigin,
            Vector3.down,
            out hit,
            footRayExtraHeight + DistanceToGround,
            layer
        );

        float yL = 0f;
        if (leftHit)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            Vector3 p = hit.point + Vector3.up * DistanceToGround;
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, p);

            Transform leftFootBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Quaternion originalRot = leftFootBone.rotation;
            Vector3 originalForward = leftFootBone.forward;

            Vector3 slopeForward = Vector3.ProjectOnPlane(-originalForward, hit.normal).normalized;
            Quaternion slopeRot = Quaternion.LookRotation(slopeForward, hit.normal);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, slopeRot);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftToes, defaultLeftToeLocalRot);
            yL = hit.point.y;
        }
        else if (!GameManager.Instance.player.IsGrounded) 
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            // 절벽 등으로 레이 못 만나면 아래로 떨어진 것으로 간주
            yL = leftOrigin.y - (footRayExtraHeight + DistanceToGround);
        }

        // RIGHT FOOT
        Vector3 rightOrigin = animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 1;
        bool rightHit = Physics.Raycast(
            rightOrigin,
            Vector3.down,
            out hit,
            footRayExtraHeight + DistanceToGround,
            layer
        );
        // && hit.transform.CompareTag("Ground")
        float yR = 0f;
        if (rightHit)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
            Vector3 p = hit.point + Vector3.up * DistanceToGround;
            animator.SetIKPosition(AvatarIKGoal.RightFoot, p);

            Transform rightFootBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Quaternion originalRot = rightFootBone.rotation;
            Vector3 originalForward = rightFootBone.forward;

            Vector3 slopeForward = Vector3.ProjectOnPlane(-originalForward, hit.normal).normalized;
            Quaternion slopeRot = Quaternion.LookRotation(slopeForward, hit.normal);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, slopeRot);
            animator.SetBoneLocalRotation(HumanBodyBones.RightToes, defaultLeftToeLocalRot);
            yR = hit.point.y;
        }
        else if (!GameManager.Instance.player.IsGrounded)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
            yR = rightOrigin.y - (footRayExtraHeight + DistanceToGround);
        }

        // 골반 드롭 계산
        float footOffset;
        if (leftHit && rightHit)
        {
            // 둘 다 닿으면 높이 차만큼
            footOffset = Mathf.Abs(yL - yR) / 2;
        }
        else if (leftHit ^ rightHit)
        {
            // 한쪽만 닿으면 고정 cliffDropAmount
            footOffset = cliffDropAmount;
        }
        else
        {
            // 둘 다 안 닿으면 평지 복귀
            footOffset = 0f;
        }

        goalHipHeight = baseHipHeight - footOffset;
    }
}
