// Scripts/Story/StoryStream.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Story Stream (Linear)", fileName = "StoryStream_Linear")]
public class StoryStream : ScriptableObject
{
    [Tooltip("이 스트림의 노드들. index 순서대로 실행됩니다.")]
    public List<Node> nodes = new();

    public List<NextStreamRule> nextStreamRules = new();
    public StoryStream defaultNextStream;
}

[Serializable]
public class Node
{
    // ====== 대화 설정 (onEnter) ======
    [Header("Dialogue (On Enter)")]
    public bool playDialogueOnEnter;
    [Tooltip("Resources 경로, 예: NPCDialogues/Hamburger")]
    public string enterDialogueFile;
    [Tooltip("그룹명, 예: Start, Order, End")]
    public string enterDialogueGroup;
    [Tooltip("대사가 끝날 때까지 기다릴지 여부")]
    public bool waitEnterDialogue = true;

    // ====== 대화 설정 (onClear) ======
    [Header("Dialogue (On Clear)")]
    public bool playDialogueOnClear;
    public string clearDialogueFile;
    public string clearDialogueGroup;
    public bool waitClearDialogue = true;

    // ====== 나머지 이벤트/조건 ======
    [Header("Events / Conditions")]
    [Tooltip("노드 입장 시 실행할 기타 이벤트 (대사 제외)")]
    public List<GameEventSO> onEnter = new();

    [Tooltip("노드 완료 시 실행할 기타 이벤트 (대사 제외)")]
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
    public List<ConditionSO> conditions = new();
    public StoryStream nextStream;
}
