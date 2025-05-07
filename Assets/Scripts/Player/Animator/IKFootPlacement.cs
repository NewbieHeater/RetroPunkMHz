using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    Animator animator;

    public LayerMask layer;

    [Range(0f, 1f)]
    public float DistanceToGround;
    [Range(0f, 2f)]
    public float fuck;
    [Range(0f, 45f)] public float maxToeBendAngle = 30f;  // 토우가 기울일 최대 각도
    [Range(0f, 1f)] public float toeBendFalloffStart = 15f; // 경사각 이하면 full, 이 각도부터 줄어듦
    private Transform leftFootBone, rightFootBone, leftToeBone, rightToeBone;

    void Start()
    {
        animator = GetComponent<Animator>();
        rightToeBone = animator.GetBoneTransform(HumanBodyBones.RightToes);
        leftToeBone = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        defaultLeftToeLocalRot = animator.GetBoneTransform(HumanBodyBones.LeftToes).localRotation;
        defaultRightToeLocalRot = animator.GetBoneTransform(HumanBodyBones.RightToes).localRotation;
        hipOffset = transform.localPosition.y;
        vel = 1f;
    }
    float hipOffset;
    float goal;
    float vel;
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            goal = transform.localPosition.y - 0.1f;
            Debug.Log(goal);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            goal = transform.localPosition.y + 0.1f;
        }
        hipOffset = Mathf.SmoothDamp(hipOffset, goal, ref vel, 0.35f,Mathf.Infinity);
        transform.localPosition = Vector3.up * hipOffset;
    }
    private Quaternion defaultLeftToeLocalRot;
    private Quaternion defaultRightToeLocalRot;
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("LeftFootIKWeight"));
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("LeftFootIKWeight"));
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("RightFootIKWeight"));
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("RightFootIKWeight"));

            //animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            //animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            //animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            //왼쪽발
            RaycastHit hit;
            Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 0.1f, Vector3.down * 0.5f);

            if (Physics.Raycast(ray, out hit, DistanceToGround + fuck, layer))
            {
                if (hit.transform.CompareTag("Ground"))
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    Quaternion footRot = Quaternion.LookRotation(forward, hit.normal);

                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(forward, hit.normal));

                    Quaternion footWorldRot = animator.GetIKRotation(AvatarIKGoal.LeftFoot);

                    Quaternion footLocalRot = Quaternion.Inverse(transform.rotation) * footWorldRot;

                    animator.SetBoneLocalRotation(
                        HumanBodyBones.LeftToes,
                        defaultRightToeLocalRot
                    );
                }
            }

            ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 0.1f, Vector3.down * 0.5f);

            if (Physics.Raycast(ray, out hit, DistanceToGround + fuck, layer))
            {
                if (hit.transform.CompareTag("Ground"))
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    Quaternion footRot = Quaternion.LookRotation(forward, hit.normal);

                    animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(forward, hit.normal));

                    Quaternion footWorldRot = animator.GetIKRotation(AvatarIKGoal.LeftFoot);

                    Quaternion footLocalRot = Quaternion.Inverse(transform.rotation) * footWorldRot;

                    animator.SetBoneLocalRotation(
                        HumanBodyBones.RightToes,
                        defaultRightToeLocalRot
                    );
                }
            }
        }
    }
}
