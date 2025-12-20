using System;
using UnityEngine;
[Serializable]
public class DebugLogAction : StoryAction
{
    [TextArea]
    public string message;
    public string message2;
    public override void Execute(StoryFlowRunner runner)
    {
        Debug.Log($"[StoryFlow] {message}");
        CinemachineEventReader.Instance.PlayBuiltInEvent(BuiltInEvents.Dialogue, message, message2);
    }
}
