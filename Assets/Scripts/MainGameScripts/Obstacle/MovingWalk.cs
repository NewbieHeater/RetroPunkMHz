using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWalk : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    private Vector3 direction;
    private List<Rigidbody> riders = new List<Rigidbody>();

    private void Awake()
    {
        direction = transform.forward;
    }

    private void OnCollisionEnter(Collision col)
    {
        var rb = col.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
            riders.Add(rb);
    }

    private void OnCollisionExit(Collision col)
    {
        var rb = col.collider.attachedRigidbody;
        if (rb != null)
            riders.Remove(rb);
    }

    private void Update()
    {
        // 충돌 중인 모든 리지드바디에 대해 MovePosition 호출
        foreach (var rb in riders)
        {
            Vector3 target = rb.position + direction.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(target);
        }
    }
}
