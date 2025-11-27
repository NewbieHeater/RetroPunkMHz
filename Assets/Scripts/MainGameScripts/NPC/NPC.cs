using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : InteractableBase
{
    public string npcId;
    public string nameForDialogue;

    protected override bool OnInteract()
    {
        Debug.Log("A");

        StoryFlowRunner.Instance.NotifyNpcTalked(npcId);

        // ① 지금 활성화된 스토리 상태에서
        //    이 NPC가 어떤 대사를 써야 하는지 물어본다.
        if (StoryFlowRunner.Instance.TryGetDialogueForNpc(npcId,
            out string fileName, out string groupName))
        {
            // ② CinemachineEventReader로 템플릿 이벤트 실행
            CinemachineEventReader.Instance.PlayDialogueSequence(fileName, groupName);
            return true;
        }
        else
        {
            // 기본 대사 (서브 NPC, 틈새 대사 등)
            CinemachineEventReader.Instance.PlayDialogueSequence(nameForDialogue, npcId);
            return true;
        }
        
    }


}
