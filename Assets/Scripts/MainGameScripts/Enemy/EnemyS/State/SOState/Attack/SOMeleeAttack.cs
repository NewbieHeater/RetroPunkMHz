using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOMeleeAttack", menuName = "Enemy Logic/Attack Logic/Melee")]
public class SOMeleeAttack : EnemyAttackSOBase
{
    enum Phase { Windup, Strike, Cooldown }
    public float windupTime = 0.5f;
    public float strikeDuration = 0.8f;
    public float cooldownTime = 0.9f;
    public float range = 3f;

    private Phase phase;
    private float phaseStart;
    private bool hasHit;


    public override void Initialize(GameObject gameObject, EnemyFSMBase enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;
        this.animator = enemy.anime;
        playerTransform = GameManager.Instance.player.transform;
    }

    public override void OperateEnter()
    {
        phase = Phase.Windup;
        phaseStart = Time.time;
        hasHit = false;
        if (enemy != null)
            enemy.meleeAttackCollider.enabled = false;
    }

    public override void OperateUpdate()
    {
        float elapsed = Time.time - phaseStart;
        switch (phase)
        {
            case Phase.Windup:
                RotateTowardPlayer();
                if (elapsed >= windupTime)
                    EnterStrike();
                break;

            case Phase.Strike:
                if (!hasHit)
                {
                    enemy.meleeAttackCollider.enabled = true;
                    hasHit = true;
                }
                if (elapsed >= strikeDuration)
                    enemy.meleeAttackCollider.enabled = false;
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Attack") && info.normalizedTime >= 1f)
                    EnterCooldown();
                break;

            case Phase.Cooldown:
                if (elapsed >= cooldownTime)
                    OperateEnter();
                break;
        }
    }

    public override void OperateFixedUpdate()
    {

    }

    public override void OperateExit()
    {
        enemy.meleeAttackCollider.enabled = false;
        animator.CrossFade("Idle", 0.05f);
    }


    private void EnterStrike()
    {
        phase = Phase.Strike;
        phaseStart = Time.time;
        hasHit = false;
        animator.CrossFade("Attack", 0.1f);
    }

    private void EnterCooldown()
    {
        phase = Phase.Cooldown;
        phaseStart = Time.time;
        animator.CrossFade("Idle", 0.05f);
    }
}
