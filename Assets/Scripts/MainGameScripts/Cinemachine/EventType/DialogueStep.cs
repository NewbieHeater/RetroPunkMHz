using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/Dialogue")]
public class DialogueStep : EventStep
{
    public string fileName; public string groupName;
    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (ctx.IsCancelled()) yield break;
        yield return DialogueManager.Instance.StartDialogueAndWait(fileName, groupName);
    }
}