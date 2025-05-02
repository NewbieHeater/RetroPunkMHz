using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    public Animator animator;
    public Transform body;
    public float footSpacing = 1f;
    public LayerMask groundLayer;

    [Range(0f, 1f)]
    public float DistanceToGround;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(body.position + (body.right * footSpacing) + (body.up * 2), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, groundLayer))
        {
            //transform.position = hit.point;

            Vector3 footPosition = hit.point;
            footPosition.y += DistanceToGround;

            transform.position = footPosition;
            //animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
            //animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
        }
    }
}
