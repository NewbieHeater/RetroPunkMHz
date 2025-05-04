using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public int hp;
    public int damage;
    public float speed;

    protected virtual void attack()
    {
        Debug.Log("basic attack");
    }

    protected void smash()
    {
        Debug.Log("³¯¾Æ°¨");
    }

    protected abstract void move();
}
