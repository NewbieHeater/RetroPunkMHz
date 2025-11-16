using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Story/Condition/Has Item")]
public class HasItemCondition : StoryCondition
{
    [SerializeField] private string itemId;
    [SerializeField] private int requiredAmount = 1;

    public override bool IsMet()
    {
        if (InventoryManager.Instance == null) return false;
        int count = InventoryManager.Instance.GetItemCount(itemId);
        return count >= requiredAmount;
    }
}