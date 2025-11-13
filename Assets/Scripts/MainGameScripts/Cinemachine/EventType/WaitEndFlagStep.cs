using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/WaitEndFlag")]
public class WaitEndFlagStep : EventStep
{
    [Min(0)] public float timeout = 0f; // 0=무한 대기
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        bool done = false;
        System.Action cb = () => done = true;
        ctx.EndFlag = cb;
        if (timeout > 0f)
        {
            float t = 0f;
            while (!done && t < timeout) { t += Time.deltaTime; yield return null; }
        }
        else
        {
            while (!done) yield return null;
        }
        ctx.EndFlag = null;
    }
}