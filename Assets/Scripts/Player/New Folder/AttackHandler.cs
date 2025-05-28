using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackHandler
{
    private Transform player;
    private Camera cam;
    private GameObject hpBarParent;
    private Image hpBar;
    private TextMeshProUGUI valueText;
    private float normalDmg, minCharge, maxCharge, range, radius;
    private bool isCharging;
    private float chargeTimer;

    public AttackHandler(Transform player, Camera cam, GameObject hpBarParent, Image hpBar, TextMeshProUGUI valueText,
                         float normalDmg, float minCharge, float maxCharge, float range, float radius)
    {
        this.player = player;
        this.cam = cam;
        this.hpBarParent = hpBarParent;
        this.hpBar = hpBar;
        this.valueText = valueText;
        this.normalDmg = normalDmg;
        this.minCharge = minCharge;
        this.maxCharge = maxCharge;
        this.range = range;
        this.radius = radius;
    }

    public void ProcessInput()
    {
        //if (Input.GetMouseButtonDown(0))
            //DoAttack(normalDmg, false);

        if (Input.GetMouseButtonDown(1))
        {
            hpBarParent.SetActive(true);
            isCharging = true;
            chargeTimer = 0f;
        }

        if (isCharging && Input.GetMouseButton(1))
        {
            if (chargeTimer < maxCharge)
            {
                chargeTimer += Time.deltaTime;
                float dmg = Mathf.FloorToInt((chargeTimer * 30f) / 10f) * 10;
                valueText.text = dmg.ToString();
                hpBar.fillAmount = chargeTimer / maxCharge;
            }
        }

        if (isCharging && Input.GetMouseButtonUp(1))
        {
            bool knock = chargeTimer >= minCharge;
            //DoAttack(Mathf.FloorToInt((chargeTimer * 30f) / 10f) * 10, knock);
            hpBarParent.SetActive(false);
            hpBar.fillAmount = 0f;
            isCharging = false;
        }
    }

    //private void DoAttack(float dmg, bool knockback)
    //{
    //    Vector3 dir = GetAttackDirection();
    //    RaycastHit[] hits = Physics.SphereCastAll(player.position, radius, dir, range, LayerMask.GetMask("Enemy"));
    //    foreach (var h in hits)
    //    {
    //        var e = h.collider.GetComponent<Enemy>(); if (e == null) continue;
    //        e.TakeDamage(dmg);
    //        if (e.isDead() && knockback) e.ApplyKnockback(dir * 20f);
    //    }
    //}

    private Vector3 GetAttackDirection()
    {
        var mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(player.position).z;
        var world = cam.ScreenToWorldPoint(mp);
        Vector3 dir = world - player.position; dir.z = 0f;
        return dir.normalized;
    }
}