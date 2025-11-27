using UnityEngine;

[CreateAssetMenu(menuName = "Story/Action/Debug Log")]
public class DebugLogAction : StoryAction
{
    [TextArea]
    public string message;

    public override void Execute(StoryFlowRunner runner)
    {
        Debug.Log($"[StoryFlow] {message}");
    }
}
