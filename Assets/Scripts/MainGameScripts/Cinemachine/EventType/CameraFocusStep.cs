using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/CameraFocus")]
public class CameraFocusStep : EventStep
{
    public int cameraSlot = 0;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (ctx.IsCancelled()) yield break;
        ctx.Focusing?.FocusTo(cameraSlot);
        yield break;
    }
}