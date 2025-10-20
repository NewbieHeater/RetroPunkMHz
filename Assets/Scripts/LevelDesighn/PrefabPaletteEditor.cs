using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabPalette))]
public class PrefabPaletteEditor : Editor
{
    SerializedProperty itemsProp;

    void OnEnable()
    {
        itemsProp = serializedObject.FindProperty("items");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("프리팹, 라벨, 태그를 등록하여 빠르게 찾고 선택합니다. 즐겨찾기 체크로 핫바 후보를 관리하세요.", MessageType.Info);

        if (GUILayout.Button("태그 자동 채우기(폴더명)", GUILayout.Height(22)))
        {
            for (int i = 0; i < itemsProp.arraySize; i++)
            {
                var e = itemsProp.GetArrayElementAtIndex(i);
                var prefabProp = e.FindPropertyRelative("prefab");
                var tagProp = e.FindPropertyRelative("tag");
                if (prefabProp.objectReferenceValue)
                {
                    var path = AssetDatabase.GetAssetPath(prefabProp.objectReferenceValue);
                    var folder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                    var name = System.IO.Path.GetFileName(folder);
                    if (!string.IsNullOrEmpty(name)) tagProp.stringValue = name;
                }
            }
        }

        EditorGUILayout.Space();
        for (int i = 0; i < itemsProp.arraySize; i++)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var e = itemsProp.GetArrayElementAtIndex(i);
                var label = e.FindPropertyRelative("label");
                var tag = e.FindPropertyRelative("tag");
                var prefab = e.FindPropertyRelative("prefab");
                var icon = e.FindPropertyRelative("customIcon");
                var fav = e.FindPropertyRelative("favorite");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prefab, GUIContent.none);
                fav.boolValue = GUILayout.Toggle(fav.boolValue, new GUIContent("★", "즐겨찾기"), "Button", GUILayout.Width(28));
                if (GUILayout.Button("X", GUILayout.Width(22))) { itemsProp.DeleteArrayElementAtIndex(i); break; }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(label);
                EditorGUILayout.PropertyField(tag);
                EditorGUILayout.PropertyField(icon);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("항목 추가", GUILayout.Height(24))) itemsProp.InsertArrayElementAtIndex(itemsProp.arraySize);

        serializedObject.ApplyModifiedProperties();
    }
}