using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKnockbackable
{
    /// <summary>�˹��� ���� �� ȣ��˴ϴ�.</summary>
    void ApplyKnockback(Vector3 dir, float force);
}
