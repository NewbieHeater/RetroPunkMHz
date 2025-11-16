using UnityEngine;

public enum IntCompareOp { GreaterOrEqual, LessOrEqual, Equal, Greater, Less }

[CreateAssetMenu(menuName = "Story/Condition/Int Var Compare")]
public class IntVarCondition : StoryCondition
{
    [SerializeField] private StoryIntVar variable;
    [SerializeField] private IntCompareOp op = IntCompareOp.GreaterOrEqual;
    [SerializeField] private int target = 1;

    public override bool IsMet()
    {
        if (variable == null) return false;
        int v = variable.Value;

        switch (op)
        {
            case IntCompareOp.GreaterOrEqual: return v >= target;
            case IntCompareOp.LessOrEqual: return v <= target;
            case IntCompareOp.Equal: return v == target;
            case IntCompareOp.Greater: return v > target;
            case IntCompareOp.Less: return v < target;
        }
        return false;
    }
}
