using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/Wait")]
public class WaitStep : EventStep
{
    public float seconds = 0.5f;

    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);
        else
            yield break;
    }
}
