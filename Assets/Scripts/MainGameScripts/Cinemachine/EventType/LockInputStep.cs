using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// === 개별 스텝 예시 ===
[CreateAssetMenu(menuName = "CineEvent/Steps/LockInput")]
public class LockInputStep : EventStep
{
    public bool lockOn = true;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        ctx.LockInput?.Invoke(lockOn);
        yield break;
    }
}