using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public interface IExplosionInteract
{
    public void OnExplosionInteract(Channel channel);
}

public class Enemy : MonoBehaviour, IExplosionInteract
{
    public TextMeshProUGUI HpBar;

    [SerializeField]
    float Hp = 100;
    protected bool dead = false;
    [Header("문을 열기 위한 최소 채널 포인트")]
    public int requiredAmpPts = 4;
    public int requiredPerPts = 5;
    public int requiredWavPts = 2;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        HpBar.text = $"Hp : {Hp}";
    }

    // Update is called once per frame
    public void TakeDamage(float damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            dead = true;
        }
        HpBar.text = $"Hp : {Hp}";
    }

    public bool isDead()
    {
        return dead;
    }

    public void ApplyKnockback(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
        
    }



    protected void Explode()
    {
        // 주변 오브젝트 상호작용 예시
        Collider[] hits = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hit in hits)
        {
            var interact = hit.GetComponent<IExplosionInteract>();
            if (interact != null)
            {
                interact.OnExplosionInteract(ChannelManager.Instance.CurrentChannel);
            }
        }
        Destroy(gameObject);
    }

    public virtual void OnExplosionInteract(Channel channel)
    {
        
    }
}
