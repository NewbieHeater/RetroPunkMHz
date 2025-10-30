using System;
using UnityEngine;

public interface IPlayerStats
{
    // Base stats
    int MaxHp { get; }
    int Hp { get; }
    float AttackDamage { get; }
    float AttackSpeed { get; }
    float AttackRange { get; }
    float ChargeAttackDamage { get; }
    float CritChance { get; }       // 0~1
    float CritMultiplier { get; }   // 2.0f 등
    float MinChargeTime { get; }
    float MaxChargeTime { get; }
    float WalkSpeed { get; }
    float RunSpeed { get; }

    // Mutations
    void ApplyDamage(int amount);
    void Heal(int amount);
    void SetMoveSpeed(float walk, float run); // 필요 시

    // Events
    event Action<int, int> OnHpChanged; // (current,max)
}


public class PlayerStats : MonoBehaviour, IPlayerStats
{
    [Header("HP")]
    [SerializeField] int _maxHp = 100;
    [SerializeField] int _hp = 100;

    [Header("Combat")]
    [SerializeField] float _attackDamage = 10f;
    [SerializeField] float _attackSpeed = 2f;
    [SerializeField] float _attackRange = 1f;
    [SerializeField] float _chargeAttackDamage = 40f;
    [SerializeField, Range(0f, 1f)] float _critChance = 0.2f;
    [SerializeField] float _critMultiplier = 2f;
    [SerializeField] float minChargeTime;
    [SerializeField] float maxChargeTime;

    [Header("Move")]
    [SerializeField] float _walkSpeed = 3.5f;
    [SerializeField] float _runSpeed = 6.0f;

    public int MaxHp => _maxHp;
    public int Hp => _hp;
    public float AttackDamage => _attackDamage;
    public float AttackSpeed => _attackSpeed;
    public float AttackRange => _attackRange;
    public float ChargeAttackDamage => _chargeAttackDamage;
    public float CritChance => _critChance;
    public float CritMultiplier => _critMultiplier;
    public float MinChargeTime => minChargeTime;
    public float MaxChargeTime => maxChargeTime;

    public float WalkSpeed => _walkSpeed;
    public float RunSpeed => _runSpeed;

    public event Action<int, int> OnHpChanged;

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;
        _hp = Mathf.Max(0, _hp - amount);
        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        _hp = Mathf.Min(_maxHp, _hp + amount);
        OnHpChanged?.Invoke(_hp, _maxHp);
    }

    public void SetMoveSpeed(float walk, float run)
    {
        _walkSpeed = walk;
        _runSpeed = run;
    }
}
