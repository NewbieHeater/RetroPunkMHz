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

    /// <summary>현재 활성 상태인 노드들(여러 개 가능).</summary>
    [SerializeField] private List<StoryNode> activeNodes = new List<StoryNode>();

    [Header("Options")]
    [Tooltip("Awake 시 자동으로 flow를 시작할지 여부")]
    [SerializeField] private bool autoStartOnAwake = true;

    [Tooltip("트랜지션에 조건이 전혀 없을 때 항상 통과로 볼지 여부")]
    [SerializeField] private bool treatEmptyTransitionAsAlwaysTrue = true;

    // 노드 ID → 노드 참조 캐시
    private Dictionary<string, StoryNode> _nodeLookup = new Dictionary<string, StoryNode>();
    // 활성 노드 ID 집합
    private HashSet<string> _activeNodeIds = new HashSet<string>();

    // ---------------------------
    // 조건 평가에 사용할 런타임 상태
    // ---------------------------

    private readonly HashSet<string> _flags = new HashSet<string>();                        // RaiseFlag 용
    private readonly HashSet<string> _talkedNpcs = new HashSet<string>();                   // NotifyNpcTalked 용
    private readonly Dictionary<string, int> _enemyKillCount = new Dictionary<string, int>(); // NotifyEnemyKilled 용

    // ---------------------------
    // 외부용 이벤트
    // ---------------------------

    /// <summary>노드가 활성화될 때(비활성 → 활성) 호출.</summary>
    public event System.Action<StoryNode> NodeActivated;

    /// <summary>노드가 비활성화될 때 호출.</summary>
    public event System.Action<StoryNode> NodeDeactivated;

    /// <summary>트랜지션이 발동될 때 호출.</summary>
    public event System.Action<StoryTransition> TransitionFired;

    /// <summary>플로우가 하나 이상의 End 노드에 도달했을 때 호출.</summary>
    public event System.Action<StoryFlowRunner> FlowCompleted;

    // 외부에서 상태 변화를 알고 싶을 때 사용할 수 있는 이벤트들
    public event System.Action<string> OnFlagRaised;
    public event System.Action<string> OnNpcTalked;
    public event System.Action<string> OnEnemyKilled;

    public StoryFlow Flow => flow;
    public IReadOnlyList<StoryNode> ActiveNodes => activeNodes;

    private void Awake()
    {
        if (autoStartOnAwake && flow != null)
        {
            StartFlow(flow);
        }
    }

    private void Update()
    {
        if (flow == null || activeNodes.Count == 0)
            return;

        EvaluateTransitionsMulti();
    }

    // ==================================================
    // 플로우 시작 / 정지
    // ==================================================

    /// <summary>
    /// 지정한 StoryFlow를 시작한다.
    /// 여러 Start 노드가 있다면 전부 활성화한다.
    /// Start 표시가 없다면 첫 노드를 Start로 사용.
    /// </summary>
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

        // Start 노드들 찾기
        List<StoryNode> startNodes = new List<StoryNode>();
        if (flow.nodes != null)
        {
            foreach (var n in flow.nodes)
            {
                if (n != null && n.isStart)
                    startNodes.Add(n);
            }
        }

        // Start 표시된 게 없다면 첫 노드를 Start로 사용
        if (startNodes.Count == 0 && flow.nodes != null && flow.nodes.Count > 0)
        {
            var first = flow.nodes[0];
            first.isStart = true;
            flow.startNodeId = first.id;
            startNodes.Add(first);
        }

        // Start 노드들 활성화
        foreach (var n in startNodes)
        {
            ActivateNode(n);
        }
    }

    /// <summary>
    /// 플로우 정지/리셋.
    /// </summary>
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
    }

    // ==================================================
    // 초기 세팅
    // ==================================================

    private void BuildNodeLookup()
    {
        _nodeLookup.Clear();

        if (flow.nodes == null)
            return;

        foreach (var node in flow.nodes)
        {
            if (node == null) continue;

            if (string.IsNullOrEmpty(node.id))
                node.id = System.Guid.NewGuid().ToString("N");

            if (!_nodeLookup.ContainsKey(node.id))
                _nodeLookup.Add(node.id, node);
            else
                Debug.LogWarning($"[StoryFlowRunner] Duplicate node id '{node.id}' in flow '{flow.flowName}'.");
        }
    }

    // ==================================================
    // 노드 활성/비활성
    // ==================================================

    public void ActivateNode(StoryNode node)
    {
        if (node == null) return;
        if (_activeNodeIds.Contains(node.id)) return;

        activeNodes.Add(node);
        _activeNodeIds.Add(node.id);

        // Enter 액션
        if (node.onEnterActions != null)
        {
            foreach (var action in node.onEnterActions)
            {
                if (action == null) continue;
                try { action.Execute(this); }
                catch (System.Exception ex) { Debug.LogException(ex); }
            }
        }

        NodeActivated?.Invoke(node);

        // End 노드면 알림
        if (node.isEnd || (!string.IsNullOrEmpty(flow.endNodeId) && flow.endNodeId == node.id))
        {
            FlowCompleted?.Invoke(this);
        }
    }

    public void DeactivateNode(StoryNode node)
    {
        if (node == null) return;
        if (!_activeNodeIds.Contains(node.id)) return;

        // Exit 액션
        if (node.onExitActions != null)
        {
            foreach (var action in node.onExitActions)
            {
                if (action == null) continue;
                try { action.Execute(this); }
                catch (System.Exception ex) { Debug.LogException(ex); }
            }
        }

        activeNodes.Remove(node);
        _activeNodeIds.Remove(node.id);

        NodeDeactivated?.Invoke(node);
    }

    // ==================================================
    // 트랜지션 평가 (여러 노드 기반)
    // ==================================================

    private void EvaluateTransitionsMulti()
    {
        if (flow.transitions == null || flow.transitions.Count == 0)
            return;

        // 1) 이번 프레임 시작 시점의 활성 노드 스냅샷
        var activeIdsSnapshot = new HashSet<string>(_activeNodeIds);

        // 2) 타겟 노드별 트랜지션 목록
        Dictionary<string, List<StoryTransition>> allMap = new();
        Dictionary<string, List<StoryTransition>> metMap = new();

        foreach (var t in flow.transitions)
        {
            if (t == null) continue;
            if (string.IsNullOrEmpty(t.fromNodeId) || string.IsNullOrEmpty(t.toNodeId))
                continue;

            // 출발 노드가 현재 활성
            if (!activeIdsSnapshot.Contains(t.fromNodeId))
                continue;

            if (!_nodeLookup.ContainsKey(t.toNodeId))
                continue;

            // 모든 트랜지션
            if (!allMap.TryGetValue(t.toNodeId, out var listAll))
            {
                listAll = new List<StoryTransition>();
                allMap[t.toNodeId] = listAll;
            }
            listAll.Add(t);

            // 조건 만족 트랜지션
            if (AreConditionsMet(t))
            {
                if (!metMap.TryGetValue(t.toNodeId, out var listMet))
                {
                    listMet = new List<StoryTransition>();
                    metMap[t.toNodeId] = listMet;
                }
                listMet.Add(t);
            }
        }

        // 3) 타겟 노드별로 IncomingMode를 적용해서
        //    실제로 활성화할 노드와, 비활성화할 소스 노드 목록을 결정
        List<StoryNode> nodesToActivate = new();
        HashSet<string> sourcesToDeactivate = new();

        foreach (var kv in allMap)
        {
            string toId = kv.Key;
            var allList = kv.Value;          // 이 타겟으로 가는 모든 트랜지션
            if (allList == null || allList.Count == 0) continue;

            metMap.TryGetValue(toId, out var metList); // 조건 만족한 트랜지션들
            if (metList == null || metList.Count == 0) continue;

            var targetNode = _nodeLookup[toId];
            bool ok = false;

            switch (targetNode.incomingMode)
            {
                case IncomingTransitionMode.Any:
                    // B→D, C→D 중 하나라도 만족하면 D 활성화
                    ok = true;
                    break;

                case IncomingTransitionMode.All:
                    // B→D, C→D 모두 만족해야 D 활성화
                    ok = (metList.Count == allList.Count);
                    break;
            }

            if (ok)
            {
                // 타겟 노드 활성화 예약
                if (!_activeNodeIds.Contains(toId) && !nodesToActivate.Contains(targetNode))
                {
                    nodesToActivate.Add(targetNode);

                    // 발동된 트랜지션들 이벤트 (원하면)
                    foreach (var t in metList)
                        TransitionFired?.Invoke(t);
                }

                // ★ 이 타겟으로 가는 모든 출발 노드는 완료로 보고 비활성화 예약
                //    (Any/All 상관없이, D가 열렸으면 B, C 같은 소스는 끝났다고 처리)
                foreach (var t in allList)
                {
                    if (!string.IsNullOrEmpty(t.fromNodeId))
                        sourcesToDeactivate.Add(t.fromNodeId);
                }
            }
        }

        // 4) 실제 노드 활성/비활성 적용

        // 새로 활성화
        foreach (var node in nodesToActivate)
        {
            ActivateNode(node);
        }

        // 출발 노드 비활성화
        foreach (var srcId in sourcesToDeactivate)
        {
            if (_nodeLookup.TryGetValue(srcId, out var srcNode))
            {
                // 아직 활성 상태인 경우에만
                if (_activeNodeIds.Contains(srcId))
                {
                    DeactivateNode(srcNode);
                }
            }
        }
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
        if (cond == null)
            return true;

        switch (cond.type)
        {
            case StoryConditionType.None:
                return true;

            case StoryConditionType.FlagTrue:
                // stringArg = 플래그 이름
                return _flags.Contains(cond.stringArg);

            case StoryConditionType.NpcTalked:
                // stringArg = NPC ID
                return _talkedNpcs.Contains(cond.stringArg);

            case StoryConditionType.EnemyKilled:
                // stringArg = Enemy ID, intArg = 필요 킬 수
                if (_enemyKillCount.TryGetValue(cond.stringArg, out int count))
                    return count >= cond.intArg;
                return false;

            default:
                return false;
        }
    }

    // ==================================================
    // 외부에서 조건 상태를 갱신하는 API
    // ==================================================

    public void RaiseFlag(string flagId)
    {
        if (string.IsNullOrEmpty(flagId))
            return;

        if (_flags.Add(flagId))
        {
            OnFlagRaised?.Invoke(flagId);
        }
    }

    public void NotifyNpcTalked(string npcId)
    {
        if (string.IsNullOrEmpty(npcId))
            return;

        if (_talkedNpcs.Add(npcId))
        {
            OnNpcTalked?.Invoke(npcId);
        }
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
    // NPC가 말할 대사를 가져오는곳
    // ==================================================
    public bool TryGetDialogueForNpc(string npcId, out string fileName, out string groupName)
    {
        // 활성 노드들 중에서 우선순위를 정해서 탐색
        // 간단히는 순서대로 첫 번째 매칭
        foreach (var node in activeNodes)
        {
            if (node.npcDialogues == null) continue;

            foreach (var entry in node.npcDialogues)
            {
                if (entry.npcId == npcId)
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

}
