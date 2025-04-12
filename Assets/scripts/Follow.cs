using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{

    [Header("따라갈 스피드 조정")]
    [Range(5f,20f)][SerializeField] float chaseSpeed;
    public Transform target;
    public Vector3 offset;


    void Update()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * chaseSpeed);
        transform.position = smoothedPosition;
    }
}
