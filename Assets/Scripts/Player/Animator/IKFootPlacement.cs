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
    public Transform body;
    public float footSpacing = 1f;
    void Start()
    {
        animator = GetComponent<Animator>();
        rightToeBone = animator.GetBoneTransform(HumanBodyBones.RightToes);
        leftToeBone = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        defaultLeftToeLocalRot = animator.GetBoneTransform(HumanBodyBones.LeftToes).localRotation;
        defaultRightToeLocalRot = animator.GetBoneTransform(HumanBodyBones.RightToes).localRotation;
        hipOffset = transform.localPosition.y;
        vel = 1f;
        goal = hipOffset;
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
    void OnDrawGizmos()
    {
        if (body == null) return;

        Vector3 origin = body.position + (body.right * footSpacing) + body.forward * 0.5f + body.up;
        Vector3 dir = Vector3.down;
        float dist = 10f;

        // Ray 경로를 파란 선으로
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + dir * dist);

        // Raycast 히트 지점을 빨간 구로
        if (Physics.Raycast(origin, dir, out RaycastHit hit1, dist, layer))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit1.point, 0.1f);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
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


        if (animator)
        {
            //animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("LeftFootIKWeight"));
            //animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("LeftFootIKWeight"));
            //animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("RightFootIKWeight"));
            //animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("RightFootIKWeight"));

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            //왼쪽발
            RaycastHit hit;
            Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 0.1f, Vector3.down);

            if (Physics.Raycast(ray, out hit, DistanceToGround + fuck, layer))
            {
                if (hit.transform.CompareTag("Ground"))
                {
                    if (Mathf.Abs(hit.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y) < 1)
                    {
                        hipOffset = Mathf.SmoothDamp(hipOffset, goal - Mathf.Abs(hit.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y), ref vel, 0.35f, Mathf.Infinity);
                        transform.localPosition = Vector3.up * hipOffset;
                        Debug.Log(Mathf.Abs(hit.point.y - animator.GetIKPosition(AvatarIKGoal.LeftFoot).y));
                    }

                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
                    Quaternion footRot = Quaternion.LookRotation(forward, hit.normal);

                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(forward, hit.normal));

                    animator.SetBoneLocalRotation(
                        HumanBodyBones.LeftToes,
                        defaultRightToeLocalRot
                    );

                    
                }
            }

            ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 0.1f, Vector3.down);

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

                    animator.SetBoneLocalRotation(
                        HumanBodyBones.RightToes,
                        defaultRightToeLocalRot
                    );
                }
            }
        }
    }
}
