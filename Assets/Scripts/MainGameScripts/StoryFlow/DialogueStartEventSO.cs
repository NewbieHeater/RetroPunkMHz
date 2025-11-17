using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Events/Dialogue(Start Group)")]
public class DialogueStartEventSO : GameEventSO, IAsyncGameEvent
{
    [Tooltip("Resources 경로(확장자 제외), 예: NPCDialogues/Hamburger")]
    public string fileName;

    [Tooltip("대화 그룹명, 예: Start, Order, End")]
    public string groupName;

    public override void Invoke()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(fileName, groupName);
    }

    public IEnumerator InvokeRoutine()
    {
        if (DialogueManager.Instance != null)
            yield return DialogueManager.Instance.StartDialogueAndWait(fileName, groupName);
    }
}
