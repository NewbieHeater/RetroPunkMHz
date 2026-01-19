using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NPC : InteractableBase
{
    

    private string _fileName;
    public string _defaultGroupName = "Quick";

    protected override bool OnInteract()
    {
        // ① 지금 활성화된 스토리 상태에서
        //    이 NPC가 어떤 대사를 써야 하는지 물어본다.
        if (StoryFlowRunner.Instance.TryGetDialogueForNpc(npcId,
            out string fileName, out string groupName))
        {
            Debug.Log($"{fileName}{groupName}");
            CinemachineEventReader.Instance.PlayBuiltInEvent(BuiltInEvents.Dialogue, fileName, groupName);
            _fileName = fileName;
            return true;
        }
        else if(_fileName != null)
        {
            Debug.Log($"a");
            // 기본 대사 (서브 NPC, 틈새 대사 등)
            CinemachineEventReader.Instance.PlayBuiltInEvent(BuiltInEvents.Dialogue, _fileName, _defaultGroupName);
            return true;
        }
        else 
        {
            CinemachineEventReader.Instance.PlayBuiltInEvent(BuiltInEvents.Dialogue, "DefaultDialogues", npcId);
            return true;
        }

        
    }


}
