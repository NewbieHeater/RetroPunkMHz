using UnityEngine;

public abstract class StoryCondition : ScriptableObject
{
    /// <summary>조건 만족 여부 반환</summary>
    public abstract bool IsMet();
}
