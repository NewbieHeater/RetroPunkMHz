// Scripts/Story/Conditions/AlwaysTrueCondition.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Conditions/Always True")]
public class AlwaysTrueCondition : ConditionSO
{
    public override bool IsMet() => true;
}
