using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    public int Maxhp;
    public int Currenthp;
    public int damage;
    public float speed;
    public int attack;
    public float attackSpeed;
    public Rigidbody body;
    public string lastType;


    void Start()
    {
        Maxhp = 100;
        Currenthp = Maxhp;
        body = GetComponent<Rigidbody>();
    }
    void Update()
    {
       
    }
    //protected
    // virtual : 함수 정의가 있어야함
    // abstract : 함수 정의가 없어야함
    //private void OnCollistionEnter(Collision collision) : 충돌이 일어났을떄 일어나는 함수
    //public = 접근 제어자

    public virtual void TakeDamage(int damage)
    {
        Currenthp = Currenthp - damage;

        if (Currenthp <= 0 )
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log("죽음");
    }

    public virtual void LastAttack(string attackType, int chargelevel, Vector3 attackDirection)
    {
        lastType = attackType;
        Debug.Log("적이 마지막으로 받은 공격 속성: " + lastType);

        Knockback(chargelevel, attackDirection);

        if (Currenthp <= 0)
        {
            Die();
        }
    }

    public virtual void Knockback(int chargelevel, Vector3 attackDirection)
    {
        float force = 0f;

        attackDirection.Normalize();

        switch (chargelevel)
            {
            case 1:
            case 2:
                force = 1f;
                break;
            case 3:
            case 4:
                force = 3f;
                break;
            case 5:
                force = 7f;
                break;
 
        }
        body.AddForce(attackDirection*force, ForceMode.Impulse);
        Debug.Log("넉백 강도 :" + force);
    }

}
