using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Story/Main Stream State")]
public class MainStreamStateObj : ScriptableObject
{
    [Header("ID / 설명")]
    public string stateId;
    [TextArea] public string description;

    [Header("이 상태를 종료시키기 위한 클리어 조건(AND)")]
    public List<StoryCondition> clearConditions = new List<StoryCondition>();

    [Header("클리어 시 공통으로 실행할 이벤트")]
    public UnityEvent onCleared;

    [Header("분기 (위에서부터 우선순위)")]
    public List<StoryBranch> branches = new List<StoryBranch>();

    [Header("어느 분기도 만족하지 않을 때 기본 다음 상태")]
    public MainStreamStateObj defaultNextState;

    [Header("클리어 조건이 없으면 입장 즉시 클리어")]
    public bool autoClearIfNoCondition = false;
}
