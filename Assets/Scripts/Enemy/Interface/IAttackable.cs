using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    // 기존 TakeDamage(float) 대신
    void TakeDamage(in DamageInfo info);
}