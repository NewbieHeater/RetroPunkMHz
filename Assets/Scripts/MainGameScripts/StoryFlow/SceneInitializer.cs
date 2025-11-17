// Scripts/Story/SceneInitializer.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-800)]
public class SceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLoaded;
    }

    private void OnLoaded(Scene scene, LoadSceneMode mode)
    {
        // 제거표시된 엔티티는 즉시 제거
        var all = GameObject.FindObjectsOfType<SceneEntity>(true);
        foreach (var e in all)
        {
            if (!string.IsNullOrEmpty(e.Guid) && GameProgress.I.IsEntityRemoved(e.Guid))
                Destroy(e.gameObject);
        }
        // 여기에 체력/인벤토리/판매품/대사 등 적용 로직을 확장 가능
    }
}
