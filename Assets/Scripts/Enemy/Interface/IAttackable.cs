using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    // ���� TakeDamage(float) ���
    void TakeDamage(in DamageInfo info);
}