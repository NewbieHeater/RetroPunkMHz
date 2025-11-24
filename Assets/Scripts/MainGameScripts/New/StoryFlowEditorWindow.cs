#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StoryFlowEditorWindow : EditorWindow
{
    private StoryFlow _flow;

    // 그래프 스크롤 오프셋 (캔버스 좌표 기준)
    private Vector2 _graphScroll;
    private Rect _graphViewRect;

    private StoryNode _selectedNode;
    private StoryTransition _selectedTransition;

    private bool _isDraggingTransition;
    private StoryNode _dragFromNode;

    // 드래그 사선의 끝점 (그래프 좌표 기준으로 저장)
    private Vector2 _dragMousePos;

    // 중클릭 패닝 상태
    private bool _isPanning;
    private Vector2 _lastPanMousePos;

    [MenuItem("Tools/Story Flow Editor")]
    public static void Open()
    {
        GetWindow<StoryFlowEditorWindow>("Story Flow");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (_flow == null)
        {
            EditorGUILayout.HelpBox(
                "상단에서 StoryFlow 에셋을 선택하거나 New 버튼으로 새로 만든 뒤,\n" +
                "좌측 그래프 영역에서 우클릭 → Create Node 로 노드를 생성하십시오.",
                MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();

        DrawGraphArea();
        DrawInspectorArea();

        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_flow);
        }
    }

    // ---------------- Toolbar ----------------

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        _flow = (StoryFlow)EditorGUILayout.ObjectField(
            _flow,
            typeof(StoryFlow),
            false,
            GUILayout.MinWidth(200));

        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            CreateNewFlowAsset();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void CreateNewFlowAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create StoryFlow",
            "NewStoryFlow",
            "asset",
            "스토리 플로우 에셋을 저장할 위치를 선택하세요.");

        if (!string.IsNullOrEmpty(path))
        {
            var asset = ScriptableObject.CreateInstance<StoryFlow>();
            asset.flowName = System.IO.Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _flow = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }

    // ---------------- 좌표 보정 ----------------

    /// <summary>
    /// 화면(mousePosition) 좌표 → 그래프(canvas) 좌표로 변환
    /// </summary>
    private Vector2 ToGraphPos(Vector2 windowPos)
    {
        return windowPos + _graphScroll;
    }

    // ---------------- Graph Area ----------------

    private void DrawGraphArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        // 1) 그래프 영역용 Rect 확보 (레이아웃 기준)
        _graphViewRect = GUILayoutUtility.GetRect(
            GUIContent.none,
            GUIStyle.none,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        // 2) 스크롤뷰 그리기 전에, 중클릭 패닝 등 이벤트 먼저 처리
        HandleGraphEvents(Event.current);

        // 3) 실제 캔버스(내용) Rect
        Rect canvasRect = new Rect(0, 0, 4000, 4000);

        // 4) 스크롤뷰 시작 (GUI.BeginScrollView 사용)
        _graphScroll = GUI.BeginScrollView(
            _graphViewRect,   // 화면에 보이는 영역
            _graphScroll,     // 현재 스크롤
            canvasRect);      // 실제 내용 크기

        // 캔버스 배경
        GUI.Box(canvasRect, GUIContent.none);

        if (_flow.nodes == null)
            _flow.nodes = new List<StoryNode>();

        BeginWindows();
        for (int i = 0; i < _flow.nodes.Count; i++)
        {
            StoryNode node = _flow.nodes[i];
            if (node == null) continue;

            if (string.IsNullOrEmpty(node.id))
            {
                node.id = System.Guid.NewGuid().ToString("N");
                EditorUtility.SetDirty(_flow);
            }

            node.editorRect = GUI.Window(
                i,
                node.editorRect,
                id => DrawNodeWindow(id, node),
                node.displayName);
        }
        EndWindows();

        DrawTransitions();
        DrawTransitionPreview();

        if (_flow.nodes.Count == 0)
        {
            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "그래프 영역에서 우클릭 → Create Node 로 첫 노드를 추가하십시오.",
                MessageType.Info);
        }

        // 5) 스크롤뷰 끝
        GUI.EndScrollView();

        EditorGUILayout.EndVertical();
    }


    private void DrawNodeWindow(int windowId, StoryNode node)
    {
        Event e = Event.current;

        // 1. 드래그 가능한 헤더 영역 정의 (윈도우 로컬 좌표)
        Rect dragRect = new Rect(0, 0, node.editorRect.width, 20f);

        // 헤더 배경 (선택 사항, 안 해도 됨)
        GUI.Box(dragRect, GUIContent.none);

        // 2. 헤더 영역은 항상 드래그 가능 (좌클릭 기준)
        GUI.DragWindow(dragRect);

        // 3. 헤더 아래부터는 실제 컨텐츠 그리기 (레이아웃 밀어내기)
        GUILayout.Space(24f);   // 헤더 높이 + 여유

        // 좌클릭으로 선택 처리 (이건 컨텐츠와 관계)
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            _selectedNode = node;
            _selectedTransition = null;
            GUI.FocusControl(null);
            // 여기서 e.Use()는 여전히 하지 않는 것이 좋음
        }

        // === 아래부터는 기존 UI 컨트롤들 ===
        EditorGUI.BeginChangeCheck();
        string newName = EditorGUILayout.TextField("Name", node.displayName);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_flow, "Rename Node");
            node.displayName = newName;
        }

        EditorGUI.BeginChangeCheck();
        bool newIsStart = EditorGUILayout.ToggleLeft("Start", node.isStart);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_flow, "Set Start Node");
            if (newIsStart)
            {
                foreach (StoryNode n in _flow.nodes)
                {
                    if (n == null) continue;
                    n.isStart = false;
                }
                node.isStart = true;
                _flow.startNodeId = node.id;
            }
            else
            {
                node.isStart = false;
                if (_flow.startNodeId == node.id)
                    _flow.startNodeId = null;
            }
        }

        EditorGUI.BeginChangeCheck();
        bool newIsEnd = EditorGUILayout.ToggleLeft("End", node.isEnd);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_flow, "Set End Node");
            if (newIsEnd)
            {
                foreach (StoryNode n in _flow.nodes)
                {
                    if (n == null) continue;
                    n.isEnd = false;
                }
                node.isEnd = true;
                _flow.endNodeId = node.id;
            }
            else
            {
                node.isEnd = false;
                if (_flow.endNodeId == node.id)
                    _flow.endNodeId = null;
            }
        }

        EditorGUI.BeginChangeCheck();
        node.incomingMode = (IncomingTransitionMode)EditorGUILayout.EnumPopup("Incoming Mode", node.incomingMode);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_flow, "Change Incoming Mode");
        }

        DrawOutputPort(node);

        // ★ 기존 맨 아래의 GUI.DragWindow(); 는 제거하거나, dragRect로 한정해야 합니다.
        // GUI.DragWindow();  // <= 이건 이제 빼는 것이 안전 gpt최고!
    }


    private void DrawOutputPort(StoryNode node)
    {
        Rect portRect = new Rect(
            node.editorRect.width - 24f,
            node.editorRect.height * 0.5f - 8f,
            18f,
            18f);

        if (GUI.Button(portRect, "▶"))
        {
            _dragFromNode = node;
            _isDraggingTransition = true;
            _dragMousePos = ToGraphPos(Event.current.mousePosition);
            _selectedTransition = null;
        }
    }

    private void DrawTransitions()
    {
        if (_flow.transitions == null)
            return;

        Handles.BeginGUI();

        foreach (StoryTransition t in _flow.transitions)
        {
            if (t == null) continue;

            StoryNode from = _flow.GetNodeById(t.fromNodeId);
            StoryNode to = _flow.GetNodeById(t.toNodeId);
            if (from == null || to == null) continue;

            Rect fromRect = from.editorRect;
            Rect toRect = to.editorRect;

            Vector3 startPos = new Vector3(fromRect.xMax, fromRect.center.y, 0);
            Vector3 endPos = new Vector3(toRect.xMin, toRect.center.y, 0);
            Vector3 startTan = startPos + Vector3.right * 40f;
            Vector3 endTan = endPos + Vector3.left * 40f;

            Color color = (t == _selectedTransition) ? Color.yellow : Color.white;

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2f);
        }

        Handles.EndGUI();
    }

    private void DrawTransitionPreview()
    {
        if (!_isDraggingTransition || _dragFromNode == null)
            return;

        Handles.BeginGUI();

        Rect fromRect = _dragFromNode.editorRect;
        Vector3 startPos = new Vector3(fromRect.xMax, fromRect.center.y, 0);
        Vector3 endPos = _dragMousePos;
        Vector3 startTan = startPos + Vector3.right * 40f;
        Vector3 endTan = endPos + Vector3.left * 40f;

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.cyan, null, 2f);

        Handles.EndGUI();
    }

    private void HandleGraphEvents(Event e)
    {
        // 그래프 영역 밖이면 무시
        if (!_graphViewRect.Contains(e.mousePosition))
            return;

        // ---------------- 중클릭 패닝 처리 ----------------
        if (e.button == 2)
        {
            if (e.type == EventType.MouseDown)
            {
                _isPanning = true;
                _lastPanMousePos = e.mousePosition;  // 윈도우(에디터 창) 좌표
                e.Use();
                return;
            }

            if (_isPanning && e.type == EventType.MouseDrag)
            {
                Vector2 delta = e.mousePosition - _lastPanMousePos;

                // 화면을 이동시키려면 스크롤은 반대 방향
                _graphScroll -= delta;

                _lastPanMousePos = e.mousePosition;

                Repaint();
                e.Use();
                return;
            }

            if (_isPanning && e.type == EventType.MouseUp)
            {
                _isPanning = false;
                e.Use();
                return;
            }
        }

        // 이 아래에 원래 쓰던
        // - 노드 선택
        // - 트랜지션 선택
        // - Delete키 삭제
        // - 우클릭 컨텍스트 메뉴
        // 등을 이어서 두면 됩니다.

        Vector2 graphPos = ToGraphPos(e.mousePosition);

        // 예시)
        // if (e.type == EventType.ContextClick)
        // {
        //     ShowContextMenu(graphPos);
        //     e.Use();
        // }
    }


    private StoryNode GetNodeAtPosition(Vector2 graphPos)
    {
        if (_flow.nodes == null)
            return null;

        for (int i = _flow.nodes.Count - 1; i >= 0; i--)
        {
            StoryNode node = _flow.nodes[i];
            if (node == null) continue;

            if (node.editorRect.Contains(graphPos))
                return node;
        }
        return null;
    }

    private StoryTransition GetTransitionAtPosition(Vector2 graphPos)
    {
        if (_flow.transitions == null)
            return null;

        const float maxDist = 8f;
        StoryTransition best = null;
        float bestDist = maxDist;

        foreach (StoryTransition t in _flow.transitions)
        {
            if (t == null) continue;

            StoryNode from = _flow.GetNodeById(t.fromNodeId);
            StoryNode to = _flow.GetNodeById(t.toNodeId);
            if (from == null || to == null) continue;

            Rect fromRect = from.editorRect;
            Rect toRect = to.editorRect;

            Vector3 startPos = new Vector3(fromRect.xMax, fromRect.center.y, 0);
            Vector3 endPos = new Vector3(toRect.xMin, toRect.center.y, 0);
            Vector3 startTan = startPos + Vector3.right * 40f;
            Vector3 endTan = endPos + Vector3.left * 40f;

            float dist = HandleUtility.DistancePointBezier(graphPos, startPos, endPos, startTan, endTan);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }

        return best;
    }

    private void ShowContextMenu(Vector2 graphPos)
    {
        GenericMenu menu = new GenericMenu();
        StoryNode nodeUnderMouse = GetNodeAtPosition(graphPos);

        if (nodeUnderMouse != null)
        {
            menu.AddItem(new GUIContent("Create Child Node"), false, () =>
            {
                CreateNode(graphPos + new Vector2(40, 40), nodeUnderMouse);
            });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Delete Node"), false, () =>
            {
                DeleteNode(nodeUnderMouse);
                Repaint();
            });
        }
        else
        {
            if (_selectedNode != null)
            {
                menu.AddItem(new GUIContent("Delete Selected Node"), false, () =>
                {
                    DeleteNode(_selectedNode);
                    Repaint();
                });
            }
            if (_selectedTransition != null)
            {
                menu.AddItem(new GUIContent("Delete Selected Transition"), false, () =>
                {
                    RemoveTransition(_selectedTransition);
                    Repaint();
                });
            }
            if (_selectedNode == null && _selectedTransition == null)
            {
                menu.AddDisabledItem(new GUIContent("Delete Selected"));
            }

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Create Node"), false, () =>
            {
                CreateNode(graphPos, null);
                Repaint();
            });
        }

        menu.ShowAsContext();
    }

    private void CreateNode(Vector2 graphPos, StoryNode parent)
    {
        if (_flow == null)
            return;

        Undo.RecordObject(_flow, "Create Node");

        StoryNode node = new StoryNode
        {
            id = System.Guid.NewGuid().ToString("N"),
            displayName = "Node " + _flow.nodes.Count,
            editorRect = new Rect(graphPos.x, graphPos.y, 240, 140),
            onEnterActions = new List<StoryAction>(),
            onExitActions = new List<StoryAction>()
        };

        _flow.nodes.Add(node);

        if (string.IsNullOrEmpty(_flow.startNodeId))
        {
            node.isStart = true;
            _flow.startNodeId = node.id;
        }

        if (parent != null)
        {
            CreateTransition(parent, node);
        }

        _selectedNode = node;
        _selectedTransition = null;
    }

    private void DeleteNode(StoryNode node)
    {
        if (_flow == null || node == null)
            return;

        Undo.RecordObject(_flow, "Delete Node");

        string deletedId = node.id;

        if (_flow.transitions != null)
        {
            _flow.transitions.RemoveAll(t =>
                t != null && (t.fromNodeId == deletedId || t.toNodeId == deletedId));
        }

        if (_flow.startNodeId == deletedId) _flow.startNodeId = null;
        if (_flow.endNodeId == deletedId) _flow.endNodeId = null;

        _flow.nodes.Remove(node);

        if (_selectedNode == node)
            _selectedNode = null;
    }

    private void CreateTransition(StoryNode from, StoryNode to)
    {
        if (_flow == null || from == null || to == null)
            return;

        if (_flow.transitions == null)
            _flow.transitions = new List<StoryTransition>();

        Undo.RecordObject(_flow, "Create Transition");

        StoryTransition t = new StoryTransition
        {
            id = System.Guid.NewGuid().ToString("N"),
            fromNodeId = from.id,
            toNodeId = to.id,
            requireAllConditions = true,
            conditions = new List<StoryCondition>()
        };

        _flow.transitions.Add(t);

        _selectedTransition = t;
        _selectedNode = null;
    }

    private void RemoveTransition(StoryTransition t)
    {
        if (_flow == null || t == null)
            return;

        Undo.RecordObject(_flow, "Delete Transition");
        _flow.transitions.Remove(t);
        if (_selectedTransition == t)
            _selectedTransition = null;
    }

    // ---------------- Inspector ----------------

    private void DrawInspectorArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(320));

        EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (_selectedTransition != null)
        {
            DrawTransitionInspector(_selectedTransition);
        }
        else if (_selectedNode != null)
        {
            DrawNodeInspector(_selectedNode);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "노드를 좌클릭으로 선택 후 드래그하면 위치를 이동할 수 있습니다.\n" +
                "마우스 휠 버튼(중클릭)을 누르고 드래그하면 그래프 전체를 패닝합니다.\n\n" +
                "노드를 클릭하면 노드가 선택되고,\n" +
                "선을 클릭하면 트랜지션이 선택됩니다.\n\n" +
                "선택된 노드/트랜지션은 Delete 키로 삭제할 수 있습니다.",
                MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawNodeInspector(StoryNode node)
    {
        EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("ID", node.id);
        node.displayName = EditorGUILayout.TextField("Name", node.displayName);

        EditorGUILayout.Space(4);

        bool isStart = EditorGUILayout.Toggle("Start", node.isStart);
        if (isStart != node.isStart)
        {
            Undo.RecordObject(_flow, "Set Start Node");
            if (isStart)
            {
                foreach (var n in _flow.nodes)
                {
                    if (n == null) continue;
                    n.isStart = false;
                }
                node.isStart = true;
                _flow.startNodeId = node.id;
            }
            else
            {
                node.isStart = false;
                if (_flow.startNodeId == node.id)
                    _flow.startNodeId = null;
            }
            EditorUtility.SetDirty(_flow);
        }

        bool isEnd = EditorGUILayout.Toggle("End", node.isEnd);
        if (isEnd != node.isEnd)
        {
            Undo.RecordObject(_flow, "Set End Node");
            if (isEnd)
            {
                foreach (var n in _flow.nodes)
                {
                    if (n == null) continue;
                    n.isEnd = false;
                }
                node.isEnd = true;
                _flow.endNodeId = node.id;
            }
            else
            {
                node.isEnd = false;
                if (_flow.endNodeId == node.id)
                    _flow.endNodeId = null;
            }
            EditorUtility.SetDirty(_flow);
        }

        EditorGUILayout.Space(4);

        node.incomingMode = (IncomingTransitionMode)EditorGUILayout.EnumPopup("Incoming Mode", node.incomingMode);

        EditorGUILayout.Space(8);

        DrawNpcDialogueList(node);

        EditorGUILayout.Space(8);

        DrawActionList("On Enter Actions", ref node.onEnterActions);
        EditorGUILayout.Space(4);
        DrawActionList("On Exit Actions", ref node.onExitActions);
    }

    private void DrawNpcDialogueList(StoryNode node)
    {
        if (node.npcDialogues == null)
            node.npcDialogues = new List<StoryNpcDialogue>();

        EditorGUILayout.LabelField("NPC Dialogues", EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < node.npcDialogues.Count; i++)
        {
            var entry = node.npcDialogues[i];
            if (entry == null)
            {
                entry = new StoryNpcDialogue();
                node.npcDialogues[i] = entry;
            }

            EditorGUILayout.BeginVertical("box");

            entry.npcId = EditorGUILayout.TextField("NPC Id", entry.npcId);
            entry.fileName = EditorGUILayout.TextField("File Name", entry.fileName);
            entry.groupName = EditorGUILayout.TextField("Group Name", entry.groupName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
        {
            Undo.RecordObject(_flow, "Remove NPC Dialogue");
            node.npcDialogues.RemoveAt(removeIndex);
            EditorUtility.SetDirty(_flow);
        }

        if (GUILayout.Button("Add NPC Dialogue"))
        {
            Undo.RecordObject(_flow, "Add NPC Dialogue");
            node.npcDialogues.Add(new StoryNpcDialogue());
            EditorUtility.SetDirty(_flow);
        }
    }

    private void DrawActionList(string label, ref List<StoryAction> list)
    {
        if (list == null)
            list = new List<StoryAction>();

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            list[i] = (StoryAction)EditorGUILayout.ObjectField(
                list[i],
                typeof(StoryAction),
                false);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
        {
            list.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("Add Action"))
        {
            list.Add(null);
        }
    }

    private void DrawTransitionInspector(StoryTransition t)
    {
        EditorGUILayout.LabelField("Transition", EditorStyles.boldLabel);

        StoryNode from = _flow.GetNodeById(t.fromNodeId);
        StoryNode to = _flow.GetNodeById(t.toNodeId);

        EditorGUILayout.LabelField("From", from != null ? from.displayName : "(missing)");
        EditorGUILayout.LabelField("To", to != null ? to.displayName : "(missing)");

        EditorGUILayout.Space(4);

        t.requireAllConditions = EditorGUILayout.Toggle(
            new GUIContent("Require All Conditions", "체크 시 이 트랜지션 내부의 조건들을 AND로 묶고,\n해제 시 OR로 묶습니다."),
            t.requireAllConditions);

        EditorGUILayout.Space(4);

        if (t.conditions == null)
            t.conditions = new List<StoryCondition>();

        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < t.conditions.Count; i++)
        {
            var cond = t.conditions[i] ?? new StoryCondition();
            t.conditions[i] = cond;

            EditorGUILayout.BeginVertical("box");

            cond.type = (StoryConditionType)EditorGUILayout.EnumPopup("Type", cond.type);

            switch (cond.type)
            {
                case StoryConditionType.None:
                    break;

                case StoryConditionType.FlagTrue:
                    cond.stringArg = EditorGUILayout.TextField("Flag Id", cond.stringArg);
                    break;

                case StoryConditionType.NpcTalked:
                    cond.stringArg = EditorGUILayout.TextField("NPC Id", cond.stringArg);
                    break;

                case StoryConditionType.EnemyKilled:
                    cond.stringArg = EditorGUILayout.TextField("Enemy Id", cond.stringArg);
                    cond.intArg = EditorGUILayout.IntField("Required Count", cond.intArg);
                    break;

                default:
                    cond.stringArg = EditorGUILayout.TextField("String Arg", cond.stringArg);
                    cond.intArg = EditorGUILayout.IntField("Int Arg", cond.intArg);
                    cond.floatArg = EditorGUILayout.FloatField("Float Arg", cond.floatArg);
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
        {
            t.conditions.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("Add Condition"))
        {
            t.conditions.Add(new StoryCondition { type = StoryConditionType.None });
        }

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Delete Transition"))
        {
            RemoveTransition(t);
        }
    }
}
#endif
