using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryBranch
{
    public string branchName;                      // 에디터용 라벨
    public List<StoryCondition> conditions;        // 모두 만족하면 이 브랜치로
    public MainStreamStateObj nextState;           // 분기 도착 상태
}
