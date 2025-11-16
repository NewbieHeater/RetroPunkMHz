using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/Delay")]
public class DelayStep : EventStep
{
    [Min(0)] public float seconds = 0.5f;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
    }
}