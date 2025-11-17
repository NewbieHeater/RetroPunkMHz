// Scripts/Story/Conditions/TalkToNpcCondition.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Conditions/Talk To NPC")]
public class TalkToNpcCondition : ConditionSO
{
    [SerializeField] private string npcId; // ¿¹: "ShopClerk"

    public override bool IsMet()
    {
        return GameProgress.I.HasFlag($"TalkedTo.{npcId}");
    }
}
