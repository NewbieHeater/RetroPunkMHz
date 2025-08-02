using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SORangeAttack", menuName = "Enemy Logic/Attack Logic/Range")]
public class SORangeAttack : EnemyAttackSOBase
{
    enum Phase { Windup, Strike, Cooldown }
    public float windupTime = 0.5f;
    public float strikeTime = 1f;
    public float cooldownTime = 0.4f;
    

    private Phase phase;
    private float timer;
    private bool hasFired;

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
        timer = windupTime;
        hasFired = false;
    }

    public override void OperateUpdate()
    {
        timer -= Time.deltaTime;
        switch (phase)
        {
            case Phase.Windup:
                RotateTowardPlayer();
                if (timer <= 0f)
                {
                    phase = Phase.Strike;
                    timer = strikeTime;
                    hasFired = false;
                    enemy.anime.CrossFade("Attack", 0.1f);
                }
                break;

            case Phase.Strike:
                if (!hasFired)
                {
                    Fire();
                    hasFired = true;
                }
                if (timer <= 0f)
                {
                    phase = Phase.Cooldown;
                    timer = cooldownTime;
                    enemy.anime.CrossFade("Idle", 0.05f);
                }
                break;

            case Phase.Cooldown:
                if (timer <= 0f)
                {
                    phase = Phase.Windup;
                    timer = windupTime;
                    hasFired = false;
                }
                break;
        }
    }

    public override void OperateFixedUpdate()
    {

    }

    public override void OperateExit()
    {
        animator.CrossFade("Idle", 0.05f);
    }

    public void Fire()
    {
        // 1) 스폰 위치 설정 (머리 위 +1m)
        Vector3 spawnPos = transform.position + Vector3.up * 1f;

        // 2) 플레이어 방향 계산
        Vector3 targetPos = playerTransform.position + Vector3.up * 1f; // 플레이어 머리 높이로 조정
        Vector3 dir = (targetPos - spawnPos).normalized;

        // 3) 회전(방향) 설정
        Quaternion rot = Quaternion.LookRotation(dir);

        ObjectPooler.SpawnFromPool("NormalBullet", spawnPos, rot);
    }
}
