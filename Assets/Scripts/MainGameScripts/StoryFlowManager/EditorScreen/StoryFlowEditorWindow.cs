#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;
using System;

public class StoryFlowEditorWindow : EditorWindow
{
    private StoryFlow _flow;

    private Vector2 _graphScroll;
    private Rect _graphViewRect;

    private StoryNode _selectedNode;
    private StoryTransition _selectedTransition;

    private bool _isDraggingTransition;
    private StoryNode _dragFromNode;
    private Vector2 _dragMousePos;
    private SerializedObject _so;
    private SerializedProperty _nodesProp;

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
        if (_flow != null)
        {
            if (_so == null || _so.targetObject != _flow)
            {
                _so = new SerializedObject(_flow);
                _nodesProp = _so.FindProperty("nodes");
            }
        }

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
        if (_so != null)
        {
            _so.ApplyModifiedProperties();
        }

        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
            EditorUtility.SetDirty(_flow);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        _flow = (StoryFlow)EditorGUILayout.ObjectField(
            _flow, typeof(StoryFlow), false, GUILayout.MinWidth(200));

        if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(60)))
            CreateNewFlowAsset();

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

    private Vector2 ToGraphPos(Vector2 windowPos)
    {
        return windowPos - _graphViewRect.position + _graphScroll;
    }

    private void DrawGraphArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        _graphViewRect = GUILayoutUtility.GetRect(
            GUIContent.none, GUIStyle.none,
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        HandleGraphEvents(Event.current);

        Rect canvasRect = new Rect(0, 0, 4000, 4000);

        _graphScroll = GUI.BeginScrollView(_graphViewRect, _graphScroll, canvasRect);

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
                node.id = Guid.NewGuid().ToString("N");
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

        GUI.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawNodeWindow(int windowId, StoryNode node)
    {
        Event e = Event.current;

        Rect dragRect = new Rect(0, 0, node.editorRect.width, 20f);
        GUI.Box(dragRect, GUIContent.none);
        GUI.DragWindow(dragRect);

        GUILayout.Space(10f);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            _selectedNode = node;
            _selectedTransition = null;
            GUI.FocusControl(null);
        }

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

        EditorGUILayout.Space(4);

        node.incomingMode = (IncomingTransitionMode)EditorGUILayout.EnumPopup("Incoming Mode", node.incomingMode);

        // === 추가: 노드 종료 정책 ===
        node.deactivationPolicy = (SourceDeactivationPolicy)EditorGUILayout.EnumPopup(
            "Deactivation Policy", node.deactivationPolicy);

        DrawOutputPort(node);
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
        if (_flow.transitions == null) return;

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
        if (!_isDraggingTransition || _dragFromNode == null) return;

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
        if (!_graphViewRect.Contains(e.mousePosition))
            return;

        if (e.button == 2)
        {
            if (e.type == EventType.MouseDown)
            {
                _isPanning = true;
                _lastPanMousePos = e.mousePosition;
                e.Use();
                return;
            }

            if (_isPanning && e.type == EventType.MouseDrag)
            {
                Vector2 delta = e.mousePosition - _lastPanMousePos;
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

        Vector2 graphPos = ToGraphPos(e.mousePosition);

        if (_isDraggingTransition)
        {
            if (e.type == EventType.MouseDrag || e.type == EventType.MouseMove)
            {
                _dragMousePos = graphPos;
                Repaint();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                StoryNode target = GetNodeAtPosition(graphPos);
                if (target != null && target != _dragFromNode)
                {
                    CreateTransition(_dragFromNode, target);
                }

                _isDraggingTransition = false;
                _dragFromNode = null;
                e.Use();
            }
            return;
        }

        if (e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace))
        {
            if (_selectedTransition != null)
            {
                RemoveTransition(_selectedTransition);
                Repaint();
                e.Use();
                return;
            }

            if (_selectedNode != null)
            {
                DeleteNode(_selectedNode);
                Repaint();
                e.Use();
                return;
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            StoryTransition hitTransition = GetTransitionAtPosition(graphPos);
            if (hitTransition != null)
            {
                _selectedTransition = hitTransition;
                _selectedNode = null;
                Repaint();
                return;
            }

            StoryNode clickedNode = GetNodeAtPosition(graphPos);
            if (clickedNode != null)
            {
                _selectedNode = clickedNode;
                _selectedTransition = null;
                Repaint();
                return;
            }

            _selectedNode = null;
            _selectedTransition = null;
        }

        if (e.type == EventType.ContextClick)
        {
            ShowContextMenu(graphPos);
            e.Use();
        }
    }

    private StoryNode GetNodeAtPosition(Vector2 graphPos)
    {
        if (_flow.nodes == null) return null;

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
        if (_flow.transitions == null) return null;

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
        if (_flow == null) return;

        Undo.RecordObject(_flow, "Create Node");

        StoryNode node = new StoryNode
        {
            id = Guid.NewGuid().ToString("N"),
            displayName = "Node " + _flow.nodes.Count,
            editorRect = new Rect(graphPos.x, graphPos.y, 240, 140),
            onEnterActions = new List<StoryAction>(),
            onExitActions = new List<StoryAction>(),
            npcDialogues = new List<StoryNpcDialogue>(),
            deactivationPolicy = SourceDeactivationPolicy.OnAnyOutgoingFired
        };

        _flow.nodes.Add(node);

        if (string.IsNullOrEmpty(_flow.startNodeId))
        {
            node.isStart = true;
            _flow.startNodeId = node.id;
        }

        if (parent != null)
            CreateTransition(parent, node);

        _selectedNode = node;
        _selectedTransition = null;
    }

    private void DeleteNode(StoryNode node)
    {
        if (_flow == null || node == null) return;

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
        if (_flow == null || from == null || to == null) return;

        if (_flow.transitions == null)
            _flow.transitions = new List<StoryTransition>();

        Undo.RecordObject(_flow, "Create Transition");

        StoryTransition t = new StoryTransition
        {
            id = Guid.NewGuid().ToString("N"),
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
        if (_flow == null || t == null) return;

        Undo.RecordObject(_flow, "Delete Transition");
        _flow.transitions.Remove(t);

        if (_selectedTransition == t)
            _selectedTransition = null;
    }

    private void DrawInspectorArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(320));

        EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (_selectedTransition != null)
            DrawTransitionInspector(_selectedTransition);
        else if (_selectedNode != null)
            DrawNodeInspector(_selectedNode);
        else
            EditorGUILayout.HelpBox("노드를 선택하거나 트랜지션을 선택하세요.", MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    private void DrawNodeInspector(StoryNode node)
    {
        EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("ID", node.id);
        node.displayName = EditorGUILayout.TextField("Name", node.displayName);

        EditorGUILayout.Space(4);

        node.incomingMode = (IncomingTransitionMode)EditorGUILayout.EnumPopup("Incoming Mode", node.incomingMode);

        // === 추가: 노드 종료 정책 ===
        node.deactivationPolicy = (SourceDeactivationPolicy)EditorGUILayout.EnumPopup(
            "Deactivation Policy", node.deactivationPolicy);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("NPC Dialogues", EditorStyles.boldLabel);

        if (node.npcDialogues == null)
            node.npcDialogues = new List<StoryNpcDialogue>();

        int removeIndex = -1;
        for (int i = 0; i < node.npcDialogues.Count; i++)
        {
            var entry = node.npcDialogues[i] ?? new StoryNpcDialogue();
            node.npcDialogues[i] = entry;

            EditorGUILayout.BeginVertical("box");
            entry.npcId = EditorGUILayout.TextField("NPC Id", entry.npcId);
            entry.fileName = EditorGUILayout.TextField("File Name", entry.fileName);
            entry.groupName = EditorGUILayout.TextField("Group Name", entry.groupName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                removeIndex = i;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
            node.npcDialogues.RemoveAt(removeIndex);

        if (GUILayout.Button("Add NPC Dialogue"))
            node.npcDialogues.Add(new StoryNpcDialogue());

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        DrawActionsInspectorForSelectedNode(node);

    }
    private bool _enterFoldout = true;
    private bool _exitFoldout = true;

    private void DrawActionsInspectorForSelectedNode(StoryNode node)
    {
        if (_flow == null || node == null || _so == null || _nodesProp == null)
            return;

        int nodeIndex = _flow.nodes.IndexOf(node);
        if (nodeIndex < 0) return;

        _so.Update();

        SerializedProperty nodeProp = _nodesProp.GetArrayElementAtIndex(nodeIndex);
        SerializedProperty enterProp = nodeProp.FindPropertyRelative("onEnterActions");
        SerializedProperty exitProp = nodeProp.FindPropertyRelative("onExitActions");

        // Enter
        _enterFoldout = EditorGUILayout.Foldout(_enterFoldout, "On Enter Actions", true);
        if (_enterFoldout)
            DrawActionList(enterProp);

        EditorGUILayout.Space(6);

        // Exit
        _exitFoldout = EditorGUILayout.Foldout(_exitFoldout, "On Exit Actions", true);
        if (_exitFoldout)
            DrawActionList(exitProp);

        _so.ApplyModifiedProperties();
        EditorUtility.SetDirty(_flow);
    }

    private static List<Type> _cachedActionTypes;

    private static List<Type> GetConcreteStoryActionTypes()
    {
        if (_cachedActionTypes != null) return _cachedActionTypes;

        var baseType = typeof(StoryAction);
        _cachedActionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
            })
            .Where(t => t != null
                        && baseType.IsAssignableFrom(t)
                        && !t.IsAbstract
                        && !t.IsGenericType
                        && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .ToList();

        return _cachedActionTypes;
    }

    private void DrawActionList(SerializedProperty listProp)
    {
        if (listProp == null) return;

        EditorGUILayout.BeginVertical("box");

        // Add button + type dropdown
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Action", GUILayout.Width(110)))
            {
                ShowAddActionMenu(listProp);
            }
        }

        if (listProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No actions.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        // Elements
        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty elem = listProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            using (new EditorGUILayout.HorizontalScope())
            {
                string typeName = GetManagedRefTypeName(elem);
                EditorGUILayout.LabelField($"[{i}] {typeName}", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Up", GUILayout.Width(40)) && i > 0)
                    listProp.MoveArrayElement(i, i - 1);

                if (GUILayout.Button("Down", GUILayout.Width(50)) && i < listProp.arraySize - 1)
                    listProp.MoveArrayElement(i, i + 1);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    // SerializeReference는 2번 호출로 완전 삭제되는 케이스가 있어 안전하게 처리
                    listProp.DeleteArrayElementAtIndex(i);
                    if (i < listProp.arraySize && listProp.GetArrayElementAtIndex(i).managedReferenceValue == null)
                        listProp.DeleteArrayElementAtIndex(i);

                    EditorGUILayout.EndVertical();
                    break;
                }
            }

            // Draw fields of the action instance
            EditorGUILayout.PropertyField(elem, GUIContent.none, true);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void ShowAddActionMenu(SerializedProperty listProp)
    {
        var types = GetConcreteStoryActionTypes();
        GenericMenu menu = new GenericMenu();

        if (types.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No StoryAction types found"));
            menu.ShowAsContext();
            return;
        }

        foreach (var t in types)
        {
            string menuName = t.Name;
            menu.AddItem(new GUIContent(menuName), false, () =>
            {
                int newIndex = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(newIndex);
                SerializedProperty elem = listProp.GetArrayElementAtIndex(newIndex);

                object instance = Activator.CreateInstance(t);
                elem.managedReferenceValue = instance;

                listProp.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(listProp.serializedObject.targetObject);
            });
        }

        menu.ShowAsContext();
    }

    private string GetManagedRefTypeName(SerializedProperty p)
    {
        if (p == null) return "(null)";
        // managedReferenceFullTypename: "AssemblyName TypeName"
        string full = p.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(full)) return "(None)";
        int lastSpace = full.LastIndexOf(' ');
        return (lastSpace >= 0 && lastSpace + 1 < full.Length) ? full.Substring(lastSpace + 1) : full;
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
            new GUIContent("Require All Conditions", "체크 시 AND, 해제 시 OR"),
            t.requireAllConditions);

        if (t.conditions == null)
            t.conditions = new List<StoryCondition>();

        EditorGUILayout.Space(4);
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
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                removeIndex = i;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        if (removeIndex >= 0)
            t.conditions.RemoveAt(removeIndex);

        if (GUILayout.Button("Add Condition"))
            t.conditions.Add(new StoryCondition { type = StoryConditionType.None });

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Delete Transition"))
            RemoveTransition(t);
    }
}
#endif
