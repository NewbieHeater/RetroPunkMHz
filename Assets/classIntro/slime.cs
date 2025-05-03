using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slime : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        hp = 50;
        damage = 10;
        speed = 2;
    }
    
    override protected void attack()
    {
        Debug.Log("slime attack(Damage:" + damage + ")");
    }
    
    override protected void move()
    {
        Debug.Log("기어가는중(speed:" + speed + ")");
    }

    // Update is called once per frame
    void Update()
    {
        attack();
        move();
    }
}