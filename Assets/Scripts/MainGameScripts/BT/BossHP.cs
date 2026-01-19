using UnityEngine;
using System;

public class BossHP : MonoBehaviour
{
    [Header("HP")]
    public float maxHp = 1000f;
    public float currentHp;

    public bool IsDead => currentHp <= 0f;

    //BT, UI, ÆäÀÌÁî
    public event Action<float, float> OnHpChanged;
    public event Action OnDead;

    void Awake()
    {
        currentHp = maxHp;
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Dead");
        OnDead?.Invoke();
    }
}
