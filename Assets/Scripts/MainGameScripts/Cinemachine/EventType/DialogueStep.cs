using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "CineEvent/Steps/Dialogue")]
public class DialogueStep : EventStep
{
    [Header("기본값(컨텍스트에 값이 없을 때만 사용)")]
    public string fileName;
    public string groupName;

    [Tooltip("true면 CinemachineEventContext에서 넘어온 값이 있으면 우선 사용")]
    public bool useContextOverride = true;

    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (ctx.IsCancelled()) yield break;

        string finalFile = fileName;
        string finalGroup = groupName;

        if (useContextOverride)
        {
            if (!string.IsNullOrEmpty(ctx.DialogueFileName))
                finalFile = ctx.DialogueFileName;

            if (!string.IsNullOrEmpty(ctx.DialogueGroupName))
                finalGroup = ctx.DialogueGroupName;
        }

        if (string.IsNullOrEmpty(finalFile) || string.IsNullOrEmpty(finalGroup))
        {
            Debug.LogWarning("[DialogueStep] fileName/groupName이 비어 있습니다.");
            yield break;
        }

        yield return ctx.DialogueManager.StartDialogueAndWait(finalFile, finalGroup);
    }
}
