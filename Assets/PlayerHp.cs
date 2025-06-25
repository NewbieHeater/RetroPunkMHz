using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHp : MonoBehaviour
{
    [SerializeField] private float hp = 100;


    public void ModifyHp(float atkPower)
    {
        hp -= atkPower;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyAttackCollider"))
        {
            //hp -= other
        }
    }
}
