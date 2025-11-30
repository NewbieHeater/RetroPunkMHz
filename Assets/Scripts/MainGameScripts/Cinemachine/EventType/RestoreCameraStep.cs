using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreCameraStep : EventStep
{
    public int cameraSlot = 0;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        ctx.Focusing.ToDefault();
        yield return null;
    }
}
