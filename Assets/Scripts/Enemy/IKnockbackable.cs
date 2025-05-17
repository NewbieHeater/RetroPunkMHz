using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKnockbackable
{
    /// <summary>넉백을 받을 때 호출됩니다.</summary>
    void ApplyKnockback(Vector3 dir, float force);
}
