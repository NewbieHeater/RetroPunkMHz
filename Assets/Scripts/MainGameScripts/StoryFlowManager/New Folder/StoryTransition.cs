using System;

using System.Collections.Generic;

/// <summary>
/// 노드 간 연결선. 이 안에 여러 StoryCondition(AND/OR)을 둘 수 있다.
/// </summary>
[Serializable]
public class StoryTransition
{
    public string id;           // GUID

    public string fromNodeId;
    public string toNodeId;

    /// <summary>
    /// true  : 이 트랜지션 안의 조건들(conditions 리스트)을 AND로 묶음.
    /// false : OR로 묶음.
    /// </summary>
    public bool requireAllConditions = true;

    /// <summary>
    /// 이 트랜지션을 타기 위한 조건들.
    /// </summary>
    public List<StoryCondition> conditions = new List<StoryCondition>();
}