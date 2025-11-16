using UnityEngine;

[CreateAssetMenu(menuName = "Story/Condition/Bool Var Equals")]
public class BoolVarCondition : StoryCondition
{
    [SerializeField] private StoryBoolVar variable;
    [SerializeField] private bool requiredValue = true;

    public override bool IsMet()
    {
        if (variable == null) return false;
        return variable.Value == requiredValue;
    }
}
