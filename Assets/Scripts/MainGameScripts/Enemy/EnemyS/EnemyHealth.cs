using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealth : MonoBehaviour, IAttackable
{
    [SerializeField] private float _maxHp = 100f;
    private float _hp;

    public float MaxHp
    {
        get => _maxHp;
        set
        {
            float v = Mathf.Max(1f, value);
            if (Mathf.Approximately(v, _maxHp)) return;
            _maxHp = v;
            if (_hp > _maxHp) _hp = _maxHp;
            BroadcastHpChanged();
        }
    }

    public float Hp => _hp;

    /// <summary>현재 HP / Max / 정규화(0~1)</summary>
    public event Action<float, float, float> OnHpChangedEx;

    /// <summary>피격 시 호출(죽지 않은 경우)</summary>
    public event Action<DamageInfo> OnHit;

    /// <summary>HP 0이 되었을 때 한 번 호출</summary>
    public event Action<DamageInfo> OnDied;

    private void OnEnable()
    {
        _hp = _maxHp;
        BroadcastHpChanged();
    }

    private void BroadcastHpChanged()
    {
        float norm = (_maxHp > 0f) ? (_hp / _maxHp) : 0f;
        OnHpChangedEx?.Invoke(_hp, _maxHp, norm);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        SetHp(_hp + amount);
    }

    private void SetHp(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, _maxHp);
        if (Mathf.Approximately(clamped, _hp)) return;
        _hp = clamped;
        BroadcastHpChanged();
    }

    // IAttackable 구현 (프로젝트의 IAttackable 정의에 맞춰 in 키워드 등 조정)
    void IAttackable.TakeDamage(in DamageInfo info) => ApplyDamage(info);

    public void ApplyDamage(in DamageInfo info)
    {
        if (_hp <= 0f) return; // 이미 사망

        float dmg = Mathf.Max(0f, info.Amount);
        SetHp(_hp - dmg);

        if (_hp <= 0f)
        {
            OnDied?.Invoke(info);
        }
        else
        {
            OnHit?.Invoke(info);
        }
    }
}
