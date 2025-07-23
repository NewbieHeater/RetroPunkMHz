using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidNavigation : MonoBehaviour
{
    public bool hasPath { get; private set; }
    public bool isStopped { get; set; }

    public float stoppingDistance { get; private set; }

    private float Speed;
    private Vector3 targetPos;

    public Vector3 velocity { get; set; }

    private Rigidbody rigid;


    private void Awake()
    {
        stoppingDistance = 0.01f;
        hasPath = false;
        isStopped = false;
        targetPos = transform.position;
        rigid = GetComponent<Rigidbody>();
    }

    public void SetDestination(Vector3 target)
    {
        targetPos = target;
        hasPath = true;
        isStopped = false;
    }
    public void SetSpeed(float s) => Speed = s;
    public float RemainingDistance()
    {
        return Mathf.Abs(targetPos.x - transform.position.x);
    }
    public void ResetPath()
    {
        hasPath = false;
        rigid.velocity = Vector3.zero;
    }


    private void Update()
    {
        ModifyCurPath();
        
    }
    private void FixedUpdate()
    {
        Move();
    }



    private void ModifyCurPath()
    {
        if(RemainingDistance() <= stoppingDistance)
        {
            ResetPath();
        }
    }

    private void Locate()
    {
        if (!hasPath || isStopped)
        {
            velocity = Vector3.zero;
            return;
        }
        velocity = transform.position.x < targetPos.x ? Vector3.right * Speed : Vector3.left * Speed;
    }

    private void Move()
    {
        Locate();
        rigid.velocity = velocity;
        
    }
}
