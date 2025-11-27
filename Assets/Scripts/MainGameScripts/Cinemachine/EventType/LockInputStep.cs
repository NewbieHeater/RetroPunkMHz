using Game.Controls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockInputStep : EventStep
{
    public bool lockOn;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        GlobalInputRouter.Instance.LockInput(true);
        yield return null;
    }

}
