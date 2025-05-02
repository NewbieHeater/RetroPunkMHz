using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    Animator animator;

    public LayerMask layer;

    [Range(0f, 1f)]
    public float DistanceToGround;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    
    void Update()
    {

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            //¿ÞÂÊ¹ß
            RaycastHit hit;
            Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            if(Physics.Raycast(ray, out hit, DistanceToGround + 1f, layer))
            {
                if (hit.transform.CompareTag("Ground"))
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }

            ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, layer))
            {
                if (hit.transform.CompareTag("Ground"))
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
        }
    }
}
