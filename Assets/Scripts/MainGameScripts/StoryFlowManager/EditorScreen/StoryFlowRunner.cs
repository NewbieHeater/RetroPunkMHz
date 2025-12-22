using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 여러 StoryNode가 동시에 활성 상태가 될 수 있는 스토리 플로우 러너.
/// - activeNodes 리스트에 여러 노드가 동시에 존재할 수 있음(여러 퀘스트 동시 진행).
/// - 매 프레임, "현재 활성 노드들"에서 나가는 트랜지션을 전부 검사.
/// - 타겟 노드(D)의 incomingMode에 따라
///   - Any: B→D, C→D 중 하나라도 조건 만족하면 D 활성화
///   - All: B→D, C→D 모두 조건 만족해야 D 활성화
/// </summary>
public class StoryFlowRunner : Singleton<StoryFlowRunner>
{
    [Header("Flow")]
    [SerializeField] private StoryFlow flow;

    [SerializeField] private List<StoryNode> activeNodes = new List<StoryNode>();

    [Header("Options")]
    [SerializeField] private bool autoStartOnAwake = true;
    [SerializeField] private bool treatEmptyTransitionAsAlwaysTrue = true;

    // node lookup
    private Dictionary<string, StoryNode> _nodeLookup = new Dictionary<string, StoryNode>();
    private HashSet<string> _activeNodeIds = new HashSet<string>();

    // ---------------------------
    // 조건 평가 런타임 상태
    // ---------------------------
    private readonly HashSet<string> _flags = new HashSet<string>();
    private readonly HashSet<string> _talkedNpcs = new HashSet<string>();
    private readonly Dictionary<string, int> _enemyKillCount = new Dictionary<string, int>();

    // ---------------------------
    // "소스 outgoing이 발동되었는지" 누적 기록
    // srcNodeId -> firedTransitionIds
    // ---------------------------
    private readonly Dictionary<string, HashSet<string>> _firedOutgoingBySource = new Dictionary<string, HashSet<string>>();
    // toNodeId -> firedTransitionIds
    private readonly Dictionary<string, HashSet<string>> _firedIncomingByTarget
        = new Dictionary<string, HashSet<string>>();

    // ---------------------------
    // 외부 이벤트
    // ---------------------------
    public event Action<StoryNode> NodeActivated;
    public event Action<StoryNode> NodeDeactivated;
    public event Action<StoryTransition> TransitionFired;
    public event Action<StoryFlowRunner> FlowCompleted;

    public event Action<string> OnFlagRaised;
    public event Action<string> OnNpcTalked;
    public event Action<string> OnEnemyKilled;

    public StoryFlow Flow => flow;
    public IReadOnlyList<StoryNode> ActiveNodes => activeNodes;

    protected override void Awake()
    {
        base.Awake();
        if (autoStartOnAwake && flow != null)
            StartFlow(flow);
    }

    private void Update()
    {
        if (flow == null || activeNodes.Count == 0)
            return;

        EvaluateTransitionsMulti();
    }

    // ==================================================
    // Flow start/stop
    // ==================================================
    public void StartFlow(StoryFlow newFlow)
    {
        if (newFlow == null)
        {
            Debug.LogWarning("[StoryFlowRunner] StartFlow called with null flow.");
            return;
        }

        flow = newFlow;
        BuildNodeLookup();
        ResetRuntimeState();

        activeNodes.Clear();
        _activeNodeIds.Clear();

        // find start nodes
        List<StoryNode> startNodes = new List<StoryNode>();
        if (flow.nodes != null)
        {
            foreach (var n in flow.nodes)
            {
                if (n != null && n.isStart)
                    startNodes.Add(n);
            }
        }

        // fallback: first node becomes start
        if (startNodes.Count == 0 && flow.nodes != null && flow.nodes.Count > 0)
        {
            var first = flow.nodes[0];
            if (first != null)
            {
                first.isStart = true;
                flow.startNodeId = first.id;
                startNodes.Add(first);
            }
        }

        foreach (var n in startNodes)
            ActivateNode(n);
    }

    public void StopFlow()
    {
        activeNodes.Clear();
        _activeNodeIds.Clear();
        ResetRuntimeState();
    }

    private void ResetRuntimeState()
    {
        _flags.Clear();
        _talkedNpcs.Clear();
        _enemyKillCount.Clear();
        _firedOutgoingBySource.Clear();

        _firedIncomingByTarget.Clear();
    }


    private void BuildNodeLookup()
    {
        _nodeLookup.Clear();

        if (flow == null || flow.nodes == null)
            return;

        foreach (var node in flow.nodes)
        {
            if (node == null) continue;

            if (string.IsNullOrEmpty(node.id))
                node.id = Guid.NewGuid().ToString("N");

            if (!_nodeLookup.ContainsKey(node.id))
                _nodeLookup.Add(node.id, node);
            else
                Debug.LogWarning($"[StoryFlowRunner] Duplicate node id '{node.id}' in flow '{flow.flowName}'.");
        }
    }

    // ==================================================
    // Node activate/deactivate
    // ==================================================
    public void ActivateNode(StoryNode node)
    {
        if (node == null) return;
        if (_activeNodeIds.Contains(node.id)) return;

        activeNodes.Add(node);
        _activeNodeIds.Add(node.id);

        // enter actions
        if (node.onEnterActions != null)
        {
            foreach (var action in node.onEnterActions)
            {
                if (action == null) continue;
                try { action.Execute(this); }
                catch (Exception ex) { Debug.LogException(ex); }
            }
        }

        NodeActivated?.Invoke(node);

        // flow completed?
        if (node.isEnd || (!string.IsNullOrEmpty(flow.endNodeId) && flow.endNodeId == node.id))
        {
            FlowCompleted?.Invoke(this);
        }
    }

    public void DeactivateNode(StoryNode node)
    {
        if (node == null) return;
        if (!_activeNodeIds.Contains(node.id)) return;

        // exit actions
        if (node.onExitActions != null)
        {
            foreach (var action in node.onExitActions)
            {
                if (action == null) continue;
                try { action.Execute(this); }
                catch (Exception ex) { Debug.LogException(ex); }
            }
        }

        activeNodes.Remove(node);
        _activeNodeIds.Remove(node.id);

        NodeDeactivated?.Invoke(node);
    }

    // ==================================================
    // Transition evaluation (multi-active)
    // ==================================================
    private void EvaluateTransitionsMulti()
    {
        if (flow == null || flow.transitions == null || flow.transitions.Count == 0)
            return;

        // 1) 이번 프레임 시작 시점 활성 노드 스냅샷
        var activeIdsSnapshot = new HashSet<string>(_activeNodeIds);

        // 이번 프레임에 새로 Fire된 트랜지션의 target 후보
        var candidateTargets = new HashSet<string>();

        // 2) 활성 소스에서 조건을 만족하는 트랜지션을 "한 번만" Fire + 기록
        foreach (var t in flow.transitions)
        {
            if (t == null) continue;
            if (string.IsNullOrEmpty(t.fromNodeId) || string.IsNullOrEmpty(t.toNodeId))
                continue;

            // 양 끝 노드가 실제 존재해야 처리
            if (!_nodeLookup.ContainsKey(t.fromNodeId) || !_nodeLookup.ContainsKey(t.toNodeId))
                continue;

            // source가 이번 프레임 시작 시점에 활성인 경우만 평가
            if (!activeIdsSnapshot.Contains(t.fromNodeId))
                continue;

            // id 보장
            if (string.IsNullOrEmpty(t.id))
                t.id = Guid.NewGuid().ToString("N");

            // 이미 Fire된 트랜지션이면 스킵 (중복 Fire 방지)
            if (IsTransitionAlreadyFired(t))
                continue;

            // 조건 평가
            if (!AreConditionsMet(t))
                continue;

            // 트랜지션 Fire(누적 기록)
            RecordFiredOutgoing(t);
            RecordFiredIncoming(t);
            candidateTargets.Add(t.toNodeId);

            TransitionFired?.Invoke(t);
        }

        // 3) IncomingMode에 따라 타겟 활성화 판정
        var nodesToActivate = new List<StoryNode>();

        foreach (var toId in candidateTargets)
        {
            // 이미 활성인 노드는 스킵
            if (_activeNodeIds.Contains(toId))
                continue;

            if (!_nodeLookup.TryGetValue(toId, out var targetNode) || targetNode == null)
                continue;

            int totalIncoming = CountValidIncomingTransitions(toId);
            if (totalIncoming <= 0)
                continue;

            int firedIncoming = CountFiredValidIncomingTransitions(toId);

            bool ok = false;
            switch (targetNode.incomingMode)
            {
                case IncomingTransitionMode.Any:
                    ok = firedIncoming >= 1;
                    break;

                case IncomingTransitionMode.All:
                    ok = firedIncoming >= totalIncoming;
                    break;
            }

            if (ok && !nodesToActivate.Contains(targetNode))
                nodesToActivate.Add(targetNode);
        }

        // 4) 활성화 적용
        for (int i = 0; i < nodesToActivate.Count; i++)
            ActivateNode(nodesToActivate[i]);

        // 5) deactivationPolicy에 따라 소스 비활성화 적용
        var sourcesToDeactivate = EvaluateSourcesToDeactivate(activeIdsSnapshot);
        foreach (var srcId in sourcesToDeactivate)
        {
            if (_nodeLookup.TryGetValue(srcId, out var srcNode))
            {
                if (_activeNodeIds.Contains(srcId))
                    DeactivateNode(srcNode);
            }
        }
    }



    private void RecordFiredOutgoing(StoryTransition t)
    {
        if (t == null) return;
        if (string.IsNullOrEmpty(t.id)) t.id = Guid.NewGuid().ToString("N");
        if (string.IsNullOrEmpty(t.fromNodeId)) return;

        if (!_firedOutgoingBySource.TryGetValue(t.fromNodeId, out var set))
        {
            set = new HashSet<string>();
            _firedOutgoingBySource[t.fromNodeId] = set;
        }
        set.Add(t.id);
    }

    /// <summary>
    /// 현재 활성 소스 노드들의 deactivationPolicy에 따라 비활성화할 소스들을 결정.
    /// </summary>
    private HashSet<string> EvaluateSourcesToDeactivate(HashSet<string> activeIdsSnapshot)
    {
        HashSet<string> sourcesToDeactivate = new HashSet<string>();

        foreach (var srcId in activeIdsSnapshot)
        {
            if (!_nodeLookup.TryGetValue(srcId, out var srcNode) || srcNode == null)
                continue;

            switch (srcNode.deactivationPolicy)
            {
                case SourceDeactivationPolicy.None:
                    break;

                case SourceDeactivationPolicy.OnAnyOutgoingFired:
                    {
                        if (CountFiredOutgoingForDeactivationPolicy(srcId) > 0)
                            sourcesToDeactivate.Add(srcId);
                        break;
                    }

                case SourceDeactivationPolicy.OnAllOutgoingFired:
                    {
                        int totalOutgoing = CountOutgoingForDeactivationPolicy(srcId);

                        // outgoing 0개면 즉시 종료
                        if (totalOutgoing == 0)
                        {
                            sourcesToDeactivate.Add(srcId);
                            break;
                        }

                        int firedCount = CountFiredOutgoingForDeactivationPolicy(srcId);
                        if (firedCount >= totalOutgoing)
                            sourcesToDeactivate.Add(srcId);

                        break;
                    }
            }
        }

        return sourcesToDeactivate;
    }


    /// <summary>
    /// srcId에서 나가는 트랜지션 중 "유효한 outgoing" 개수:
    /// - fromNodeId == srcId
    /// - toNodeId가 존재하고, lookup에 존재하는 노드여야 함
    /// </summary>
    private int CountValidOutgoingTransitions(string srcId)
    {
        if (flow == null || flow.transitions == null) return 0;

        int count = 0;
        foreach (var tr in flow.transitions)
        {
            if (tr == null) continue;
            if (tr.fromNodeId != srcId) continue;

            if (string.IsNullOrEmpty(tr.toNodeId)) continue;
            if (!_nodeLookup.ContainsKey(tr.toNodeId)) continue;

            count++;
        }
        return count;
    }

    private bool AreConditionsMet(StoryTransition t)
    {
        if (t.conditions == null || t.conditions.Count == 0)
            return treatEmptyTransitionAsAlwaysTrue;

        if (t.requireAllConditions)
        {
            // AND
            for (int i = 0; i < t.conditions.Count; i++)
            {
                if (!CheckCondition(t.conditions[i]))
                    return false;
            }
            return true;
        }
        else
        {
            // OR
            for (int i = 0; i < t.conditions.Count; i++)
            {
                if (CheckCondition(t.conditions[i]))
                    return true;
            }
            return false;
        }
    }

    private bool CheckCondition(StoryCondition cond)
    {
        if (cond == null) return true;

        switch (cond.type)
        {
            case StoryConditionType.None:
                return true;

            case StoryConditionType.FlagTrue:
                return _flags.Contains(cond.stringArg);

            case StoryConditionType.NpcTalked:
                return _talkedNpcs.Contains(cond.stringArg);

            case StoryConditionType.EnemyKilled:
                if (_enemyKillCount.TryGetValue(cond.stringArg, out int count))
                    return count >= cond.intArg;
                return false;

            default:
                return false;
        }
    }

    // ==================================================
    // External state update APIs
    // ==================================================
    public void RaiseFlag(string flagId)
    {
        if (string.IsNullOrEmpty(flagId))
            return;

        if (_flags.Add(flagId))
            OnFlagRaised?.Invoke(flagId);
    }

    public void NotifyNpcTalked(string npcId)
    {
        if (string.IsNullOrEmpty(npcId))
            return;

        if (_talkedNpcs.Add(npcId))
            OnNpcTalked?.Invoke(npcId);
    }

    public void NotifyEnemyKilled(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId))
            return;

        if (_enemyKillCount.TryGetValue(enemyId, out int cur))
            _enemyKillCount[enemyId] = cur + 1;
        else
            _enemyKillCount[enemyId] = 1;

        OnEnemyKilled?.Invoke(enemyId);
    }

    // ==================================================
    // Dialogue query
    // ==================================================
    public bool TryGetDialogueForNpc(string npcId, out string fileName, out string groupName)
    {
        // 기본: 활성 노드들 중 "최근 활성화된 노드 우선"으로 매칭
        for (int i = activeNodes.Count - 1; i >= 0; i--)
        {
            var node = activeNodes[i];
            if (node == null || node.npcDialogues == null) continue;

            foreach (var entry in node.npcDialogues)
            {
                if (entry != null && entry.npcId == npcId)
                {
                    fileName = entry.fileName;
                    groupName = entry.groupName;
                    return true;
                }
            }
        }

        fileName = null;
        groupName = null;
        return false;
    }

    private void RecordFiredIncoming(StoryTransition t)
    {
        if (t == null) return;
        if (string.IsNullOrEmpty(t.id)) t.id = Guid.NewGuid().ToString("N");
        if (string.IsNullOrEmpty(t.toNodeId)) return;

        if (!_firedIncomingByTarget.TryGetValue(t.toNodeId, out var set))
        {
            set = new HashSet<string>();
            _firedIncomingByTarget[t.toNodeId] = set;
        }
        set.Add(t.id);
    }

    private bool IsTransitionAlreadyFired(StoryTransition t)
    {
        if (t == null) return false;
        if (string.IsNullOrEmpty(t.fromNodeId)) return false;
        if (string.IsNullOrEmpty(t.id)) return false;

        return _firedOutgoingBySource.TryGetValue(t.fromNodeId, out var set) && set.Contains(t.id);
    }

    /// <summary>
    /// toId로 들어오는 "유효한 incoming 트랜지션" 개수
    /// - toNodeId == toId
    /// - fromNodeId 존재 + lookup에 존재
    /// </summary>
    private int CountValidIncomingTransitions(string toId)
    {
        if (flow == null || flow.transitions == null) return 0;
        if (string.IsNullOrEmpty(toId)) return 0;
        if (!_nodeLookup.ContainsKey(toId)) return 0;

        int count = 0;
        foreach (var tr in flow.transitions)
        {
            if (tr == null) continue;
            if (tr.toNodeId != toId) continue;

            if (string.IsNullOrEmpty(tr.fromNodeId)) continue;
            if (!_nodeLookup.ContainsKey(tr.fromNodeId)) continue;

            count++;
        }
        return count;
    }

    /// <summary>
    /// toId로 들어오는 incoming 중, 실제로 Fire된(기록된) "유효한" 개수
    /// </summary>
    private int CountFiredValidIncomingTransitions(string toId)
    {
        if (flow == null || flow.transitions == null) return 0;
        if (string.IsNullOrEmpty(toId)) return 0;

        if (!_firedIncomingByTarget.TryGetValue(toId, out var firedSet) || firedSet == null)
            return 0;

        int count = 0;
        foreach (var tr in flow.transitions)
        {
            if (tr == null) continue;
            if (tr.toNodeId != toId) continue;

            if (string.IsNullOrEmpty(tr.id)) continue;
            if (firedSet.Contains(tr.id))
                count++;
        }
        return count;
    }

    private int CountFiredValidOutgoingTransitions(string srcId)
    {
        if (flow == null || flow.transitions == null) return 0;
        if (string.IsNullOrEmpty(srcId)) return 0;

        if (!_firedOutgoingBySource.TryGetValue(srcId, out var firedSet) || firedSet == null)
            return 0;

        int count = 0;
        foreach (var tr in flow.transitions)
        {
            if (tr == null) continue;
            if (tr.fromNodeId != srcId) continue;

            if (string.IsNullOrEmpty(tr.toNodeId)) continue;
            if (!_nodeLookup.ContainsKey(tr.toNodeId)) continue;

            if (string.IsNullOrEmpty(tr.id)) continue;
            if (firedSet.Contains(tr.id))
                count++;
        }
        return count;
    }

    /// <summary>
    /// DeactivationPolicy 판단용 outgoing 개수 (보수적으로 셈)
    /// - fromNodeId == srcId
    /// - toNodeId가 비어있지만 않으면 카운트 (lookup 존재 여부는 무시)
    /// </summary>
    private int CountOutgoingForDeactivationPolicy(string srcId)
    {
        if (flow == null || flow.transitions == null) return 0;

        int count = 0;
        foreach (var tr in flow.transitions)
        {
            if (tr == null) continue;
            if (tr.fromNodeId != srcId) continue;
            if (string.IsNullOrEmpty(tr.toNodeId)) continue;

            count++;
        }
        return count;
    }

    /// <summary>
    /// DeactivationPolicy 판단용 fired outgoing 개수 (보수적으로 셈)
    /// - CountOutgoingForDeactivationPolicy에 해당하는 트랜지션 중 fired인 것만 카운트
    /// </summary>
    private int CountFiredOutgoingForDeactivationPolicy(string srcId)
    {
        if (flow == null || flow.transitions == null) return 0;

        if (!_firedOutgoingBySource.TryGetValue(srcId, out var firedSet) || firedSet == null)
            return 0;

        int count = 0;
        foreach (var tr in flow.transitions)
        {
            if (tr == null) continue;
            if (tr.fromNodeId != srcId) continue;
            if (string.IsNullOrEmpty(tr.toNodeId)) continue;
            if (string.IsNullOrEmpty(tr.id)) continue;

            if (firedSet.Contains(tr.id))
                count++;
        }
        return count;
    }

}