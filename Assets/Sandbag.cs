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

    // takedamage�� ����鿡�� ��� x enemy ������ ���
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
        Debug.Log("������� ���� ������ ���� �Ӽ�: " + lastType);

        if (Currenthp / Maxhp <= 0.2f)
        {
            Knockback(chargelevel, attackDirection);
        }
        else
        {
            Debug.Log("ü���� 20%���ϰ� �ƴ�");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
