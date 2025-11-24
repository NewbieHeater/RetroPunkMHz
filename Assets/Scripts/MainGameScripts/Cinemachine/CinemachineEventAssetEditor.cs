#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(CinemachineEventAsset))]
public class CinemachineEventAssetEditor : Editor
{
    private ReorderableList _list;
    private SerializedProperty _stepsProp;

    private void OnEnable()
    {
        _stepsProp = serializedObject.FindProperty("steps");

        _list = new ReorderableList(
            serializedObject,
            _stepsProp,
            draggable: true,
            displayHeader: true,
            displayAddButton: true,
            displayRemoveButton: true
        );

        _list.drawHeaderCallback = r =>
        {
            EditorGUI.LabelField(r, "Steps");
        };

        _list.elementHeight = EditorGUIUtility.singleLineHeight + 6f;

        _list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = _stepsProp.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, element, GUIContent.none);
        };

        // + 버튼으로 "단일 스텝" 드롭다운 추가
        _list.onAddDropdownCallback = (buttonRect, list) =>
        {
            ShowAddSingleStepMenu();
        };

        // 삭제 시 서브에셋도 같이 삭제
        _list.onRemoveCallback = list =>
        {
            if (list.index < 0 || list.index >= _stepsProp.arraySize)
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                return;
            }

            var element = _stepsProp.GetArrayElementAtIndex(list.index);
            var obj = element.objectReferenceValue;

            if (obj != null)
            {
                Undo.DestroyObjectImmediate(obj);
            }

            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockPlayerInputWhileRunning"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("postDelay"));

        EditorGUILayout.Space();
        _list.DoLayoutList();

        using (new EditorGUILayout.HorizontalScope())
        {
            // 전체 시퀀스 추가
            if (GUILayout.Button("Add Sequence (Lock/Focus/Dialog/Wait/Restore)"))
            {
                AddTemplateSequence();
            }

            // 단일 스텝 추가용 버튼 (위의 + 드롭다운과 동일 메뉴)
            if (GUILayout.Button("Add Single Step..."))
            {
                ShowAddSingleStepMenu();
            }
        }

        if (GUILayout.Button("CheckValidate"))
        {
            serializedObject.ApplyModifiedProperties();
            ValidateAsset();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // =================================================================
    // 단일 스텝 추가 메뉴
    // =================================================================
    private void ShowAddSingleStepMenu()
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("Lock Input"), false, () => CreateAndAddStep<LockInputStep>());
        menu.AddItem(new GUIContent("Camera Focus"), false, () => CreateAndAddStep<CameraFocusStep>());
        menu.AddItem(new GUIContent("Dialogue"), false, () => CreateAndAddStep<DialogueStep>());
        menu.AddItem(new GUIContent("Wait"), false, () => CreateAndAddStep<WaitStep>());
        //menu.AddItem(new GUIContent("Restore Camera"), false, () => CreateAndAddStep<RestoreCameraStep>());

        menu.ShowAsContext();
    }

    private void CreateAndAddStep<T>() where T : EventStep
    {
        var asset = (CinemachineEventAsset)target;
        if (asset == null) return;

        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.DisplayDialog(
                "Cinemachine Event",
                "먼저 CinemachineEventAsset 을 프로젝트에 에셋으로 저장해야 합니다.",
                "OK"
            );
            return;
        }

        var step = ScriptableObject.CreateInstance<T>();
        step.name = typeof(T).Name;

        Undo.RegisterCreatedObjectUndo(step, "Create Event Step");
        AssetDatabase.AddObjectToAsset(step, asset);

        serializedObject.Update();

        int idx = _stepsProp.arraySize;
        _stepsProp.InsertArrayElementAtIndex(idx);
        var elem = _stepsProp.GetArrayElementAtIndex(idx);
        elem.objectReferenceValue = step;

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.SaveAssets();
    }

    // =================================================================
    // 전체 템플릿 시퀀스 추가
    // =================================================================
    private void AddTemplateSequence()
    {
        var asset = (CinemachineEventAsset)target;
        if (asset == null) return;

        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.DisplayDialog(
                "Cinemachine Event",
                "먼저 CinemachineEventAsset 을 프로젝트에 에셋으로 저장해야 합니다.",
                "OK"
            );
            return;
        }

        serializedObject.Update();

        // 1) LockInput
        var lockStep = ScriptableObject.CreateInstance<LockInputStep>();
        lockStep.name = "LockInputStep";
        lockStep.lockOn = true;
        AssetDatabase.AddObjectToAsset(lockStep, asset);
        AddStepReference(lockStep);

        // 2) CameraFocus
        var focusStep = ScriptableObject.CreateInstance<CameraFocusStep>();
        focusStep.name = "CameraFocusStep";
        focusStep.cameraSlot = 0;
        AssetDatabase.AddObjectToAsset(focusStep, asset);
        AddStepReference(focusStep);

        // 3) Dialogue
        var dialogStep = ScriptableObject.CreateInstance<DialogueStep>();
        dialogStep.name = "DialogueStep";
        dialogStep.fileName = "";
        dialogStep.groupName = "";
        AssetDatabase.AddObjectToAsset(dialogStep, asset);
        AddStepReference(dialogStep);

        // 4) Wait
        var waitStep = ScriptableObject.CreateInstance<WaitStep>();
        waitStep.name = "WaitStep";
        waitStep.seconds = 0.5f;
        AssetDatabase.AddObjectToAsset(waitStep, asset);
        AddStepReference(waitStep);

        // 5) Restore
        //var restoreStep = ScriptableObject.CreateInstance<RestoreCameraStep>();
        //restoreStep.name = "RestoreCameraStep";
        //AssetDatabase.AddObjectToAsset(restoreStep, asset);
        //AddStepReference(restoreStep);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(asset);
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.SaveAssets();
    }

    private void AddStepReference(EventStep step)
    {
        int idx = _stepsProp.arraySize;
        _stepsProp.InsertArrayElementAtIndex(idx);
        var elem = _stepsProp.GetArrayElementAtIndex(idx);
        elem.objectReferenceValue = step;
    }

    // =================================================================
    // Validate (간단 검증)
    // =================================================================
    private void ValidateAsset()
    {
        var asset = (CinemachineEventAsset)target;
        if (asset == null) return;

        var sb = new StringBuilder();

        if (asset.steps == null || asset.steps.Count == 0)
        {
            sb.AppendLine("• Step 이 하나도 없습니다.");
        }
        else
        {
            for (int i = 0; i < asset.steps.Count; i++)
            {
                var s = asset.steps[i];
                if (s == null)
                {
                    sb.AppendLine($"• Step {i}: null 참조입니다.");
                    continue;
                }

                if (s is CameraFocusStep focus && focus.cameraSlot < 0)
                    sb.AppendLine($"• Step {i} (CameraFocusStep): cameraSlot < 0 입니다.");

                if (s is DialogueStep dlg)
                {
                    if (string.IsNullOrEmpty(dlg.fileName))
                        sb.AppendLine($"• Step {i} (DialogueStep): fileName 이 비어 있습니다.");
                    if (string.IsNullOrEmpty(dlg.groupName))
                        sb.AppendLine($"• Step {i} (DialogueStep): groupName 이 비어 있습니다.");
                }

                if (s is WaitStep wait && wait.seconds <= 0f)
                    sb.AppendLine($"• Step {i} (WaitStep): seconds <= 0 입니다.");
            }
        }

        if (sb.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Cinemachine Event - Validate",
                "문제가 발견되지 않았습니다.",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Cinemachine Event - Validate",
                sb.ToString(),
                "OK"
            );
        }
    }
}
#endif