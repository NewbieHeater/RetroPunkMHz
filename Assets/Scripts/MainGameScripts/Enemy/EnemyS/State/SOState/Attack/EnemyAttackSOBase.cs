using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackSOBase : ScriptableObject
{
    protected EnemyFSMBase enemy;
    protected Transform transform;
    protected GameObject gameObject;
    protected Animator animator;

    protected Transform playerTransform;
    public float rotationSpeed = 360f;
    public virtual void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;
        this.animator = enemy.anime;
        playerTransform = GameManager.Instance.player.transform;
    }

    public virtual void OperateEnter()
    {
        
    }

    public virtual void OperateUpdate()
    {
        
    }

    public virtual void OperateFixedUpdate()
    {

    }

    public virtual void OperateExit()
    {

    }

    protected void RotateTowardPlayer()
    {
        Vector3 dir = (enemy.player.transform.position - transform.position).normalized;
        Quaternion tgt = Quaternion.LookRotation(dir);
        float newY = Mathf.MoveTowardsAngle(
            transform.eulerAngles.y,
            tgt.eulerAngles.y,
            rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, newY, transform.eulerAngles.z);
    }
}
