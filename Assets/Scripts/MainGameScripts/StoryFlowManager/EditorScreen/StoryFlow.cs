using System;
using System.Collections.Generic;
using UnityEngine;

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
        if (string.IsNullOrEmpty(id) || nodes == null) return null;

        foreach (var n in nodes)
        {
            if (n != null && n.id == id) return n;
        }
        return null;
    }
}

public enum SourceDeactivationPolicy
{
    None,
    OnAnyOutgoingFired,
    OnAllOutgoingFired
}

public enum IncomingTransitionMode
{
    Any,
    All
}

[Serializable]
public class StoryNpcDialogue
{
    public string npcId;     // ¿¹: "NPC_GuardTown"
    public string fileName;  // ¿¹: "Main_01"
    public string groupName; // ¿¹: "GateGuard_Intro"
}

[Serializable]
public class StoryNode
{
    public string id;
    public string displayName;

    public bool isStart;
    public bool isEnd;

    public IncomingTransitionMode incomingMode = IncomingTransitionMode.Any;
    public SourceDeactivationPolicy deactivationPolicy = SourceDeactivationPolicy.OnAnyOutgoingFired;

    public Rect editorRect = new Rect(100, 100, 240, 140);

    [SerializeReference] public List<StoryAction> onEnterActions = new();
    [SerializeReference] public List<StoryAction> onExitActions = new();

    public List<StoryNpcDialogue> npcDialogues = new();
}
