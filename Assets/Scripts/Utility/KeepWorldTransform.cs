using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class KeepWorldTransform : MonoBehaviour
{
    public bool lockPosition = true;
    public bool lockRotation = false;
    public bool lockScale = false;

    [SerializeField] private Vector3 capturedWorldPosition;
    [SerializeField] private Quaternion capturedWorldRotation;
    [SerializeField] private Vector3 capturedLocalScale;

    void OnEnable()
    {
        CaptureCurrentWorldTransform();

#if UNITY_EDITOR
        // 씬 저장 이벤트에 등록
        EditorSceneManager.sceneSaving += OnSceneSaving;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorSceneManager.sceneSaving -= OnSceneSaving;
#endif
    }

    void LateUpdate()
    {
        ApplyLock();
    }

    void OnValidate()
    {
        ApplyLock();
    }

    void ApplyLock()
    {
        if (lockPosition) transform.position = capturedWorldPosition;
        if (lockRotation) transform.rotation = capturedWorldRotation;
        if (lockScale) transform.localScale = capturedLocalScale;
    }

    [ContextMenu("Capture Current World Transform")]
    public void CaptureCurrentWorldTransform()
    {
        capturedWorldPosition = transform.position;
        capturedWorldRotation = transform.rotation;
        capturedLocalScale = transform.localScale;
    }

#if UNITY_EDITOR
    private void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        // 씬 저장 전에 다시 원래 좌표로 돌려줌
        ApplyLock();
        EditorUtility.SetDirty(transform); // 강제로 dirty 표시해서 저장
    }
#endif
}
