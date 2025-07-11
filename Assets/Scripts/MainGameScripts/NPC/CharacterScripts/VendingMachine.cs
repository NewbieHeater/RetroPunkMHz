using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Condition
{
    public string conditionName;
    public bool value;  
}
public class VendingMachine : InteractableNPCBase
{
    public string npcId;
    public bool hasMetPlayer;
    public Condition condition;

    protected override void Start()
    {
        base.Start();
        //ConditionManager.Instance.SetCondition(condition.conditionName, false);
            
    }

    

    public override string GetInteractPrompt()
    {
        return "[F] ��ȭ�ϱ�";
    }

    public override void Interact()
    {
        Debug.Log($"{gameObject.name}�� ��ȭ ����");
        
        if (!hasMetPlayer)
            DialogueManager.Instance.StartDialogue(npcId, "Quest1");
        else
            DialogueManager.Instance.StartDialogue(npcId, "QuestMinimalize1");
        hasMetPlayer = true;
    }
}
