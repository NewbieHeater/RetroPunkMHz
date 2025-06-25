using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    public enum Type { Melee, Range};
    public Type type;
    public float damage;
    public float rate;
    
    float attackTime = 0.3f;
    float changedamage;

    public BoxCollider meleeArea;
    private void Awake()
    {
        meleeArea = GameObject.Find("hand").GetComponent<BoxCollider>();
    }
    public void Swing()
    {
        changedamage = damage;
        meleeArea.enabled = true;
        Debug.Log("기본 공격! 데미지: " + changedamage);
        Invoke("DisableMeleeArea", attackTime);
    }

    public void ChargeSwing(float chargePower)
    {
        meleeArea = GameObject.Find("hand").GetComponent<BoxCollider>();
        changedamage = Mathf.Clamp(chargePower * damage, damage, 100);
        meleeArea.enabled = true;
        Debug.Log("차지 공격! 데미지: " + changedamage);
        Invoke("DisableMeleeArea", attackTime + 0.3f);
    }

    void DisableMeleeArea()
    {
        meleeArea.enabled = false;
        changedamage = damage;
    }
}
