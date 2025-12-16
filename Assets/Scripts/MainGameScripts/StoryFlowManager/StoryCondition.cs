using System;
using System.Collections.Generic;
using UnityEngine;

#region Condition Type & Condition Class

/// <summary>
/// 트랜지션(연결선)에 붙는 조건 종류.
/// 필요에 따라 계속 확장 가능.
/// </summary>
public enum StoryConditionType
{
    None,           // 항상 참
    FlagTrue,       // 특정 플래그가 true인지 (RaiseFlag)
    NpcTalked,      // 특정 NPC와 대화했는지 (NotifyNpcTalked)
    EnemyKilled,    // 특정 적을 N마리 이상 처치했는지 (NotifyEnemyKilled)
    // HasItem,     // 인벤토리 연동 시 추가
    // CustomInt,   // 커스텀 수 비교 등 필요시 추가
}

/// <summary>
/// ScriptableObject가 아닌, 순수 직렬화 클래스.
/// 트랜지션 내부에 List로 들어가고, StoryConditionType + 파라미터로 의미를 해석.
/// </summary>
[Serializable]
public class StoryCondition
{
    public StoryConditionType type;

    // 조건 종류에 따라 해석되는 공통 인자
    public string stringArg;   // Flag 이름, NPC ID, Enemy ID 등
    public int intArg;         // 필요 킬 수 등
    public float floatArg;     // 필요 시 사용
}

#endregion