using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Story Stream (Linear)", fileName = "StoryStream_Linear")]
public class StoryStream : ScriptableObject
{
    [Tooltip("이 스트림의 노드들. index 순서대로 실행됩니다.")]
    public List<Node> nodes = new();

    [Tooltip("이 스트림이 끝났을 때, 조건에 따라 다음 스트림을 고릅니다(위→아래 우선). 비워도 됩니다.")]
    public List<NextStreamRule> nextStreamRules = new();

    [Tooltip("어떤 룰도 만족하지 않을 때 이동할 기본 다음 스트림(비면 전체 진행 종료)")]
    public StoryStream defaultNextStream;
}

[Serializable]
public class Node
{
    [Tooltip("노드 입장 시 실행되는 이벤트(대사, 컷신 등)")]
    public List<GameEventSO> onEnter = new();

    [Tooltip("노드 완료 시 실행되는 이벤트(대사, 보상 등)")]
    public List<GameEventSO> onClear = new();

    [Tooltip("노드 완료를 위한 조건(AND)")]
    public List<ConditionSO> conditions = new();

    [Tooltip("노드 내부의 작은 퀘스트/서브 이벤트들")]
    public List<NodeTask> tasks = new();
}

[Serializable]
public class NodeTask
{
    public string id;
    public List<ConditionSO> conditions = new();
    public List<GameEventSO> onClear = new();
}

[Serializable]
public class NextStreamRule
{
    public List<ConditionSO> conditions = new(); // AND
    public StoryStream nextStream;
}
