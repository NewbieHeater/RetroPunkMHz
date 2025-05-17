using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheckHandler
{
    private Transform point;
    private float distance;
    private LayerMask mask;
    public bool IsGrounded { get; private set; }

    public GroundCheckHandler(Transform point, float distance, LayerMask mask)
    {
        this.point = point;
        this.distance = distance;
        this.mask = mask;
    }

    public void Tick()
    {
        IsGrounded = Physics.Raycast(point.position, Vector3.down, distance, mask);
    }
}