using UnityEngine;
using UnityEngine.SceneManagement;

public class UiSave : MonoBehaviour
{
    private static UiSave instance;
    [SerializeField] private GameObject inventoryPanel;
    private void Awake()
    {
        
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        // 처음 실행 시 인벤토리 비활성화
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        // 씬 바뀔 때마다 꺼지도록 이벤트 등록
        SceneManager.activeSceneChanged += OnSceneChanged;
    }
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // ✅ 씬이 바뀌면 인벤토리 자동 off
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);
    }
}
