using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/RestoreCamera")]
public class RestoreCameraStep : EventStep
{
    public bool toSavedSlot = true;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (toSavedSlot && ctx.SavedCameraSlot >= 0) ctx.Focusing?.FocusTo(ctx.SavedCameraSlot);
        else ctx.Focusing?.ToDefault();
        yield break;
    }
}
