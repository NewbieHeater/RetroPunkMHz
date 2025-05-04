using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sandBag : Enemy
{
    private void Start()
    {
        damage = 0;
        speed = 0;
        hp = 1000;
    }

    protected override void attack()
    {
        Debug.Log("공격 안함");
    }
    protected override void move()
    {
        Debug.Log("안움직임");
    }

    private void Update()
    {
        attack();
        move();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("공격당함");
    }
}
