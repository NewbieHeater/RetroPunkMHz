using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbag : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        Maxhp = 100;
        Currenthp = Maxhp;
    }

    // takedamage는 샌드백에서 사용 x enemy 에서만 사용
    override public void TakeDamage(int damage)
    {
        Currenthp -= damage;

        if(Currenthp < 1)
        {
            Currenthp = 1;
        }
    }

    override public void LastAttack(string attackType, int chargelevel, Vector3 attackDirection)
    {
        lastType = attackType;
        Debug.Log("샌드백이 받은 마지막 공격 속성: " + lastType);

        if (Currenthp / Maxhp <= 0.2f)
        {
            Knockback(chargelevel, attackDirection);
        }
        else
        {
            Debug.Log("체력이 20%이하가 아님");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
