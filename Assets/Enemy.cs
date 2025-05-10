using System;
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


    public virtual void Start()
    {
        Maxhp = 100;
        Currenthp = Maxhp;
        body = GetComponent<Rigidbody>();
    }
    void Update()
    {
       
    }
    //protected
    // virtual : �Լ� ���ǰ� �־����
    // abstract : �Լ� ���ǰ� �������
    //private void OnCollistionEnter(Collision collision) : �浹�� �Ͼ���� �Ͼ�� �Լ�
    //public = ���� ������

    protected virtual void TakeDamage(int damage)
    {
        Currenthp = Currenthp - damage;

        if (Currenthp <= 0 )
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log("����");
    }

    public virtual void LastAttack(string attackType, int chargelevel, Vector3 attackDirection)
    {
        lastType = attackType;
        Debug.Log("���� ���������� ���� ���� �Ӽ�: " + lastType);

        Knockback(chargelevel, attackDirection);

        if (Currenthp <= 0)
        {
            Die();
        }
    }
    //override�� �ڽ� ��ũ��Ʈ���� �����ϰ� �θ� ��ũ��Ʈ������ ���� ���Ҷ� (�θ��ص� ������ �����ؾ��� �ϴ��� �θ� ��ũ��Ʈ���� ��������)
    public void Knockback(int chargelevel, Vector3 attackDirection)
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
        Debug.Log("�˹� ���� :" + force);
    }


    //virtual ����
    //public class Animal
    //{
    //    public void Update()
    //    {
    //        if(Input.GetKeyDown(KeyCode.K) Speak();
    //    }
    //    public virtual void Speak()
    //    {
    //        Console.WriteLine("������ �Ҹ��� ���ϴ�.");
    //    }
    //}

    //public class Dog : Animal
    //{
    //    // ���������� ������
    //    public override void Speak()
    //    {
    //        Console.WriteLine("�۸�!");
    //    }
    //}

    //�̷��� �ֿܼ��� "�۸�!"�� ���ɴϴ�


    //public class Dog : Animal
    //{
    //
    //}

    //������ override�� ���� �ʰ� �׳� Speak�� ȣ���ϸ�
    //"������ �Ҹ��� ���ϴ�."��� ��µ˴ϴ�

    //----------------------------------------------------------------------------
    //���� Start�� ������ �ִٰ��߾���?
    //�ڽĿ��� Start �θ𿡵� Start�� ������ �ڽ��� Start�� �θ��� Start�� ���ܹ����ϴ�.
    //���� �ٲ�а�ó�� Start���� virtual, Override�� �ٿ��ΰ� �ڽĿ��� base.Start()�� �ؼ� �θ� Start�� �����ϰ� ���ּ���
    //----------------------------------------------------------------------------


    //���� : ���⺻ ������ ���� �ֵ�, ���Ѵٸ� �ڽ�Ŭ�������� ������ ���ɡ�









    //abstract��
    //
    //public abstract class Shape
    //{
    //    public abstract float Area();  // ���� ����, �ݵ�� override
    //}

    //public class Circle : Shape
    //{
    //    public float Radius;
    //    public override float Area()
    //    {
    //        return Mathf.PI * Radius * Radius;
    //    }
    //}

    //�ݵ�� �ڽ��� override�ؾ���


    //�� abstract �� ���°�?

    //1. ���� �ǵ� ���

    //���� �޼���� ���� Ŭ���� �������� ������ �� ����, �Ļ� Ŭ�������� �޶����߸� �Ѵ١��� �ǵ��� �и��� �巯���ϴ�.
    //�Ǽ��� ������ ������ �ʵ��� ������ Ÿ�ӿ� �����մϴ�.

    //2. �ҿ�����(�ϼ����� ����) Ŭ����
    //�߻� Ŭ������ �κ������θ� ������ ���赵�� ���� �մϴ�.
    //���� ��ü�� ���� �� �����Ƿ�, ���� �Ļ� Ŭ�������� �ϼ��� ������� ����� �� �ֽ��ϴ�. -> �ڽ� Ŭ������ ���赵

    //3. ������
    //��� �Ļ� Ŭ������ �ݵ�� Ư�� �޼��带 �����ϵ��� ���������ν�, ��Ÿ�� ����(NullReference ��) �� ������ �� �ֽ��ϴ�.

    //���� : ������ ���赵 ������ �ڽ�Ŭ������ �θ�Ŭ������ ���赵�� ���󸸵�� ����


    //�׷� ������ �ְ��� abstract�־� virtual�� �� �Ǵµ�?
    //���α׷����� ���Ǹ� ���ؼ�����
    //��� ��� �����մϴ�. ������ virtual�� �����ΰ� �ڽĽ�ũ��Ʈ���� ���� ���ع����� ������Ʈ ��ü�� ���� �͹�����?�̷��� abstract�� �Ἥ "�ݵ��" �����ϰ� �������ν� �޸տ���(�Ǽ�)�� ���̴°̴ϴ�.
    //���� ���� ���������� (public, private)�� ��޶������? �Ƚᵵ ��ũ��Ʈ ���ư��ϴ� ������ �ٸ� �����ڰ� ���� ���� �׸��� �Ǽ����� �ʰ� ���� ����� �ϴ� �̴ϴ�. abstract�� �� ���弱�ΰŰ��ƿ� 
    //���� ��Ӱ����� ����� �� �ȵǾ Ʋ�������� �־�� �� ���ͳݿ� �˻��غ���
}
