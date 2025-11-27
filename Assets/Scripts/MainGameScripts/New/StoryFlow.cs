using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 스토리 그래프 ScriptableObject.
/// </summary>
[CreateAssetMenu(menuName = "Story/Story Flow")]
public class StoryFlow : ScriptableObject
{
    public string flowName;

    public List<StoryNode> nodes = new List<StoryNode>();
    public List<StoryTransition> transitions = new List<StoryTransition>();

    public string startNodeId;
    public string endNodeId;

    public StoryNode GetNodeById(string id)
    {
        if (string.IsNullOrEmpty(id) || nodes == null)
            return null;

        foreach (var n in nodes)
        {
            if (n != null && n.id == id)
                return n;
        }
        return null;
    }
}


public enum IncomingTransitionMode
{
    Any,    // B→D, C→D 중 하나라도 만족하면 D 활성화
    All     // B→D, C→D 둘 다의 조건이 만족돼야 D 활성화
}

[Serializable]
public class StoryNpcDialogue
{
    public string npcId;     // 예: "NPC_GuardTown"
    public string fileName;  // 예: "Main_01"
    public string groupName; // 예: "GateGuard_Intro"
}

[Serializable]
public class StoryNode
{
    public string id;
    public string displayName;

    public bool isStart;
    public bool isEnd;
    public IncomingTransitionMode incomingMode;

    public Rect editorRect = new Rect(100, 100, 220, 80);

    public List<StoryAction> onEnterActions = new();
    public List<StoryAction> onExitActions = new();

    // ★ 여기: 반드시 public 이거나 [SerializeField] + [Serializable] 이어야 함
    public List<StoryNpcDialogue> npcDialogues = new();
}


