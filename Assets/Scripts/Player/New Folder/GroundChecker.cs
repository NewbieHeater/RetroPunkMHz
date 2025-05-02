using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public Transform groundCheck;
    public float groundDistance = 0.5f;
    public LayerMask groundLayer;
    public bool IsGrounded { get; private set; }

    public void CheckGround()
    {
        IsGrounded = Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            groundDistance,
            groundLayer
        );
    }
}
