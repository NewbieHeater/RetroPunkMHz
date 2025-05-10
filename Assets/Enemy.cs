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
    // virtual : 함수 정의가 있어야함
    // abstract : 함수 정의가 없어야함
    //private void OnCollistionEnter(Collision collision) : 충돌이 일어났을떄 일어나는 함수
    //public = 접근 제어자

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
    //override는 자식 스크립트에서 구현하고 부모 스크립트에서는 구현 안할때 (부모가해도 되지만 적게해야함 일단은 부모 스크립트에서 하지말죠)
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
        Debug.Log("넉백 강도 :" + force);
    }


    //virtual 예시
    //public class Animal
    //{
    //    public void Update()
    //    {
    //        if(Input.GetKeyDown(KeyCode.K) Speak();
    //    }
    //    public virtual void Speak()
    //    {
    //        Console.WriteLine("동물이 소리를 냅니다.");
    //    }
    //}

    //public class Dog : Animal
    //{
    //    // 선택적으로 재정의
    //    public override void Speak()
    //    {
    //        Console.WriteLine("멍멍!");
    //    }
    //}

    //이러면 콘솔에는 "멍멍!"만 나옵니다


    //public class Dog : Animal
    //{
    //
    //}

    //하지만 override를 하지 않고 그냥 Speak를 호출하면
    //"동물이 소리를 냅니다."라고 출력됩니다

    //----------------------------------------------------------------------------
    //전에 Start에 문제가 있다고했었죠?
    //자식에도 Start 부모에도 Start가 있으면 자식의 Start가 부모의 Start를 숨겨버립니다.
    //제가 바꿔둔것처럼 Start에도 virtual, Override를 붙여두고 자식에서 base.Start()를 해서 부모도 Start를 실행하게 해주세요
    //----------------------------------------------------------------------------


    //정리 : “기본 동작을 정해 주되, 원한다면 자식클래스에서 재정의 가능”









    //abstract란
    //
    //public abstract class Shape
    //{
    //    public abstract float Area();  // 구현 없음, 반드시 override
    //}

    //public class Circle : Shape
    //{
    //    public float Radius;
    //    public override float Area()
    //    {
    //        return Mathf.PI * Radius * Radius;
    //    }
    //}

    //반드시 자식이 override해야함


    //왜 abstract 를 쓰는가?

    //1. 설계 의도 명시

    //“이 메서드는 기초 클래스 차원에서 구현할 수 없고, 파생 클래스마다 달라져야만 한다”는 의도를 분명히 드러냅니다.
    //실수로 구현을 빼먹지 않도록 컴파일 타임에 강제합니다.

    //2. 불완전한(완성되지 않은) 클래스
    //추상 클래스는 부분적으로만 구현된 설계도로 봐야 합니다.
    //직접 객체를 만들 수 없으므로, 오직 파생 클래스만이 완성된 기능으로 사용할 수 있습니다. -> 자식 클래스의 설계도

    //3. 안정성
    //모든 파생 클래스가 반드시 특정 메서드를 구현하도록 강제함으로써, 런타임 누락(NullReference 등) 을 방지할 수 있습니다.

    //정리 : 일종의 설계도 무조건 자식클래스가 부모클래스의 설계도를 따라만들게 강제


    //그럼 질문이 있겠죠 abstract왜씀 virtual로 다 되는데?
    //프로그래머의 편의를 위해서에요
    //사실 없어도 무방합니다. 하지만 virtual로 만들어두고 자식스크립트에서 구현 안해버리면 프로젝트 전체가 뇌절 와버리죠?이럴때 abstract를 써서 "반드시" 구현하게 만듦으로써 휴먼에러(실수)를 줄이는겁니다.
    //전에 제가 접근한정자 (public, private)꼭 써달라고했죠? 안써도 스크립트 돌아갑니다 하지만 다른 협업자가 보기 좋게 그리고 실수하지 않게 서로 배려를 하는 겁니다. abstract도 그 연장선인거같아요 
    //제가 상속개념을 배운지 얼마 안되어서 틀렸을수도 있어요 꼭 인터넷에 검색해보기
}
