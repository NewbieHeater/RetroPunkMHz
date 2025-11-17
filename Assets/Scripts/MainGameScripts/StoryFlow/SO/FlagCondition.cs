// Scripts/Story/Conditions/FlagCondition.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Conditions/Flag")]
public class FlagCondition : ConditionSO
{
    [SerializeField] private string key;
    [SerializeField] private bool expected = true;

    public override bool IsMet()
    {
        bool has = GameProgress.I.HasFlag(key);
        return expected ? has : !has;
    }
}
