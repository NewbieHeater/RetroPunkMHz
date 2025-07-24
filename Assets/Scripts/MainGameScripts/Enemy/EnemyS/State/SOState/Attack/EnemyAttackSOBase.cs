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
    [SerializeField] private float rotationSpeedDegPerSec = 360f;
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
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f)
            return;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        Debug.Log(targetRot);
        animator.transform.rotation = Quaternion.RotateTowards(
            animator.transform.rotation,
            targetRot,
            rotationSpeedDegPerSec * Time.deltaTime
        );
    }
}
