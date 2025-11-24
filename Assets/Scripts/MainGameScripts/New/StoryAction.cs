using UnityEngine;


/// <summary>
/// 노드에 들어갈 때 / 나올 때 실행할 액션(보상, 컷씬 등).
/// 액션은 재사용성이 크기 때문에 ScriptableObject로 두는 편이 낫다.
/// </summary>
public abstract class StoryAction : ScriptableObject
{
    public abstract void Execute(StoryFlowRunner runner);
}