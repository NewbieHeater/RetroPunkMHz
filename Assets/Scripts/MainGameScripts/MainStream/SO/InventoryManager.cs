using UnityEngine;


// 예시용 인터페이스/싱글턴 스텁
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetItemCount(string itemId)
    {
        // 실제 인벤토리 로직에 맞춰 구현
        return 0;
    }
}
