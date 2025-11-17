// Scripts/Story/SceneEntity.cs
using UnityEngine;

public class SceneEntity : MonoBehaviour
{
    [Tooltip("엔티티의 전역 고유 ID. 모든 씬에서 유일해야 함.")]
    public string Guid;

    private void Start()
    {
        if (string.IsNullOrEmpty(Guid))
        {
            Debug.LogWarning($"{name} has empty Guid. Please assign a unique Guid.");
            return;
        }

        if (GameProgress.I != null && GameProgress.I.IsEntityRemoved(Guid))
            Destroy(gameObject);
    }
}
