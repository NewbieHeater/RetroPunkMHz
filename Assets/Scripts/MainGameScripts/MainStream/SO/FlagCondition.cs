using UnityEngine;

[CreateAssetMenu(menuName = "Story/Condition/Flag Activated")]
public class FlagCondition : StoryCondition
{
    [SerializeField] private MainStreamConditionFlag flag;

    public override bool IsMet()
    {
        return flag != null && flag.IsActive;
    }
}
