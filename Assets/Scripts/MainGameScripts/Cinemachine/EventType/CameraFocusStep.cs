using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocusStep : EventStep
{
    public int cameraSlot;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        ctx.Focusing.FocusTo(cameraSlot);
        yield return null;
    }

}
