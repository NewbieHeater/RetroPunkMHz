#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EventStepDTO))]
public class EventStepDTODrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, true);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var typeProp = property.FindPropertyRelative("type");
        var lockOnProp = property.FindPropertyRelative("lockOn");
        var cameraSlotProp = property.FindPropertyRelative("cameraSlot");
        var fileNameProp = property.FindPropertyRelative("fileName");
        var groupNameProp = property.FindPropertyRelative("groupName");
        var secondsProp = property.FindPropertyRelative("seconds");

        var r = position;
        r.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(r, typeProp); r.y += r.height + 2;

        var type = (StepType)typeProp.enumValueIndex;
        switch (type)
        {
            case StepType.LockInput:
                EditorGUI.PropertyField(r, lockOnProp); break;

            case StepType.CameraFocus:
                EditorGUI.PropertyField(r, cameraSlotProp); break;

            case StepType.Dialogue:
                EditorGUI.PropertyField(r, fileNameProp); r.y += r.height + 2;
                EditorGUI.PropertyField(r, groupNameProp); break;

            case StepType.WaitEndFlag:
                EditorGUI.LabelField(r, "Wait external EndEvent()"); break;

            case StepType.Delay:
                EditorGUI.PropertyField(r, secondsProp); break;

            case StepType.RestoreCamera:
                EditorGUI.LabelField(r, "Restore to default camera"); break;
        }

        EditorGUI.EndProperty();
    }
}
#endif
