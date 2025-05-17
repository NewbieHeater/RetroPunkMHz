using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour, IExplosionInteract, IKnockbackable, IAttackable
{
    public TextMeshProUGUI HpBar;

    [SerializeField]
    protected float Hp = 100;
    protected bool dead = false;


    public int RequiredAmpPts => 4;
    public int RequiredPerPts => 5;
    public int RequiredWavPts => 2;

    protected Rigidbody rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        HpBar.text = $"Hp : {Hp}";
    }

    #region 피해
    public virtual void TakeDamage(float damage)
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
    #endregion

    #region 넉백
    public void ApplyKnockback(Vector3 dir, float force)
    {
        if(!isDead()) { return; }
        Vector3 knockbackForce = dir * force;
        rb.AddForce(knockbackForce, ForceMode.Impulse);
    }
    #endregion

    #region 폭팔
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
    #endregion

}
