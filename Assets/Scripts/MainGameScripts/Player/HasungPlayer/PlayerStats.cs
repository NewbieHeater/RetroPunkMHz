using UnityEngine;
using System;

public interface IPlayerStats
{
    int MaxHp { get; }
    int Hp { get; }
    float AttackDamage { get; }
    float AttackSpeed { get; }
    float AttackRange { get; }
    float ChargeAttackDamage { get; }
    float CritChance { get; }
    float CritMultiplier { get; }
    float MinChargeTime { get; }
    float MaxChargeTime { get; }
    float WalkSpeed { get; }
    float RunSpeed { get; }
    void ApplyDamage(int amount);
    void Heal(int amount);
    void SetMoveSpeed(float walk, float run);
    event Action<int, int> OnHpChanged;
}

public class PlayerStats : MonoBehaviour, IPlayerStats
{
    [Header("기본 HP")]
    [SerializeField] int _maxHp = 100;
    [SerializeField] int _hp = 100;

    [Header("기본 전투 능력치")]
    [SerializeField] float _baseAttackDamage = 10f;
    [SerializeField] float _baseAttackSpeed = 2f;
    [SerializeField] float _attackRange = 1f;
    [SerializeField] float _chargeAttackDamage = 40f;
    [SerializeField] float _critChance = 0.5f;
    [SerializeField] float _critMultiplier = 1.5f;

    [Header("기본 이동속도")]
    [SerializeField] float _walkSpeed = 3.5f;
    [SerializeField] float _runSpeed = 6.0f;

    public int MaxHp => _maxHp;
    public int Hp => _hp;

    public float AttackDamage
    {
        get
        {
            int amp = ChannelManager.AmpPts;
            return _baseAttackDamage * (1.0f + 0.1f * amp);
        }
    }

    public float AttackSpeed
    {
        get
        {
            int per = ChannelManager.PerPts;
            return _baseAttackSpeed * (1.0f + 0.2f * per);
        }
    }

    public float AttackRange => _attackRange;
    public float ChargeAttackDamage => _chargeAttackDamage;

    public float CritChance
    {
        get
        {
            int wav = ChannelManager.WavPts;
            float chance = _critChance + wav * 0.1f;
            return Mathf.Clamp01(chance);
        }
    }

    public float CritMultiplier => _critMultiplier;

    public float MinChargeTime => 0.3f;
    public float MaxChargeTime => 2.0f;

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

    public float CalculateFinalDamage()
    {
        float dmg = AttackDamage;
        bool isCrit = UnityEngine.Random.value < CritChance;
        if (isCrit) dmg *= CritMultiplier;
        return dmg;
    }
}
