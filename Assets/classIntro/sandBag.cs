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
        Debug.Log("���� ����");
    }
    protected override void move()
    {
        Debug.Log("�ȿ�����");
    }

    private void Update()
    {
        attack();
        move();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("���ݴ���");
    }
}
