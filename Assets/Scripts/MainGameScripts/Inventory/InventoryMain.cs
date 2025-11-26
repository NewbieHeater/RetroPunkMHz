using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryMain : Singleton<InventoryMain>
{
    // 외부 호환성 유지
    public static bool IsInventoryActive = false;

    [Header("References")]
    [SerializeField] private GameObject _inventoryBase;         // 인벤토리 루트(활/비활)
    [SerializeField] private GameObject _inventorySlotsParent;  // 슬롯 부모
    [SerializeField] private InventorySlot[] _slots;            // 캐시

    [Header("Behavior")]
    [Tooltip("Update에서 I키(or Router)로 토글을 처리할지")]
    [SerializeField] private bool handleToggleInput = true;

    [Tooltip("GlobalInputRouter를 사용해 토글(권장). 해제 시 KeyCode 기반")]
    [SerializeField] private bool useInputRouter = true;

    [Tooltip("useInputRouter=false일 때 사용할 토글 키")]
    [SerializeField] private KeyCode fallbackToggleKey = KeyCode.I;

    [Tooltip("인벤토리 열릴 때 게임 입력 잠금 여부")]
    [SerializeField] private bool lockGameInputWhileOpen = true;

    [Tooltip("씬 전환 시 자동 저장/복원 동작")]
    [SerializeField] private bool autoPersistOnSceneChange = true;

    [Tooltip("이 오브젝트를 씬 전환에서 보존할지")]
    [SerializeField] private bool dontDestroyOnLoad = false;

    // 이벤트: 외부 연동용
    public event System.Action Opened;
    public event System.Action Closed;
    public event System.Action<InventorySlot[]> SlotsChanged;

    // 내부 상태
    private bool _routerWasLockedByMe;

    public List<ItemData> savedItems = new List<ItemData>();

    protected override void Awake()
    {
        base.Awake();

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        // _inventoryBase를 먼저 끄되, 슬롯 탐색은 includeInactive=true로 처리
        if (_inventoryBase != null && _inventoryBase.activeSelf)
            _inventoryBase.SetActive(false);

        AutoResolveReferences();
        RebuildSlotsCache(); // 비활성이어도 슬롯 확보

        if (autoPersistOnSceneChange)
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        if (autoPersistOnSceneChange)
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void AutoResolveReferences()
    {
        // 슬롯 부모가 비어 있으면 베이스를 부모로 가정
        if (_inventorySlotsParent == null)
            _inventorySlotsParent = _inventoryBase;
    }

    private void RebuildSlotsCache()
    {
        if (_inventorySlotsParent != null)
            _slots = _inventorySlotsParent.GetComponentsInChildren<InventorySlot>(true); // ★ 비활성 포함
        else
            _slots = new InventorySlot[0];
    }

    private void OnValidate()
    {
        // 에디터에서 레이아웃 변경시 자동 동기화
        AutoResolveReferences();
        if (_inventorySlotsParent != null)
            _slots = _inventorySlotsParent.GetComponentsInChildren<InventorySlot>(true);
    }

    private void Update()
    {
        if (!handleToggleInput) return;
        if (GameMenuManager.IsOptionActive) return;

        bool pressed = false;

        if (useInputRouter && Game.Controls.GlobalInputRouter.Instance != null)
        {
            var f = Game.Controls.GlobalInputRouter.Instance.CurrentFrame;
            pressed = f.buttons.IsDown(Game.Controls.GameInputAction.InventoryToggle);
        }
        else
        {
            pressed = Input.GetKeyDown(fallbackToggleKey);
        }
        if (pressed)
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (!IsInventoryActive) OpenInventory();
        else CloseInventory();
    }

    /// <summary>인벤토리를 연다.</summary>
    private void OpenInventory()
    {
        if (_inventoryBase != null) _inventoryBase.SetActive(true);
        IsInventoryActive = true;

        // 커서/입력 처리
        UnlockCursor();

        if (lockGameInputWhileOpen && Game.Controls.GlobalInputRouter.Instance != null)
        {
            // 잠금 중 버튼 허용 X, 축 허용 X
            Game.Controls.GlobalInputRouter.Instance.LockInput(true, null, false);
            _routerWasLockedByMe = true;
        }

        Opened?.Invoke();
    }

    /// <summary>인벤토리를 닫는다.</summary>
    public void CloseInventory()
    {
        if (_inventoryBase != null) _inventoryBase.SetActive(false);
        IsInventoryActive = false;

        TryLockCursor();

        if (_routerWasLockedByMe && Game.Controls.GlobalInputRouter.Instance != null)
        {
            Game.Controls.GlobalInputRouter.Instance.Unlock();
            _routerWasLockedByMe = false;
        }

        Closed?.Invoke();
    }

    public void TryLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public InventorySlot[] GetAllItems() => _slots;

    // -------------------------------
    // 저장/복원
    // -------------------------------
    public void SaveFromSlots()
    {
        savedItems.Clear();

        if (_slots == null || _slots.Length == 0)
            RebuildSlotsCache();

        for (int i = 0; i < _slots.Length; i++)
        {
            var s = _slots[i];
            if (s == null) continue;

            if (s.Item != null)
                savedItems.Add(new ItemData(i, s.Item, s.ItemCount));
        }
    }

    public void LoadToSlots()
    {
        if (_slots == null || _slots.Length == 0)
            RebuildSlotsCache();

        foreach (var data in savedItems)
        {
            if (data.slotIndex < 0 || data.slotIndex >= _slots.Length) continue;
            _slots[data.slotIndex].AddItem(data.item, data.count);
        }

        SlotsChanged?.Invoke(_slots);
    }

    private void OnSceneUnloaded(Scene s)
    {
        if (!autoPersistOnSceneChange) return;
        SaveFromSlots();
        // 씬 나갈 때 UI 열려 있으면 닫아 두기(안전)
        if (IsInventoryActive) CloseInventory();
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (!autoPersistOnSceneChange) return;
        // UI 트리 재구성됐을 수 있으므로 슬롯 캐시 갱신 후 복원
        RebuildSlotsCache();
        LoadToSlots();
    }

    // -------------------------------
    // 아이템 획득
    // -------------------------------
    /// <summary>
    /// 특정 슬롯으로 아이템 넣기(가능 시 스택, 불가 시 덮어쓰기)
    /// </summary>
    public void AcquireItem(Item item, InventorySlot targetSlot, int count = 1)
    {
        if (item == null || targetSlot == null || count <= 0) return;

        // 슬롯 마스크 불허면 무시
        if (!targetSlot.IsMask(item)) return;

        if (item.CanOverlap && targetSlot.Item != null && targetSlot.Item.ItemID == item.ItemID)
        {
            targetSlot.UpdateSlotCount(count);
            SlotsChanged?.Invoke(_slots);
            return;
        }

        targetSlot.AddItem(item, count);
        SlotsChanged?.Invoke(_slots);
    }

    /// <summary>
    /// 빈 슬롯 또는 기존 스택에 자동 배치
    /// </summary>
    public void AcquireItem(Item item, int count = 1)
    {
        if (item == null || count <= 0) return;

        if (_slots == null || _slots.Length == 0)
            RebuildSlotsCache();

        // 1) 스택 가능: 동일 ID 스택에 우선 추가
        if (item.CanOverlap)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                var s = _slots[i];
                if (s == null || s.Item == null) continue;
                if (!s.IsMask(item)) continue;
                if (s.Item.ItemID == item.ItemID)
                {
                    s.UpdateSlotCount(count);
                    SlotsChanged?.Invoke(_slots);
                    return;
                }
            }
        }

        // 2) 비어 있는 적합 슬롯에 배치
        for (int i = 0; i < _slots.Length; i++)
        {
            var s = _slots[i];
            if (s == null) continue;

            if (s.Item == null && s.IsMask(item))
            {
                s.AddItem(item, count);
                SlotsChanged?.Invoke(_slots);
                return;
            }
        }

        Debug.LogWarning($"[InventoryMain] No available slot for item {item.name} x{count}.");
    }

    private static Dictionary<int, Item> _itemCache;

    /// <summary>
    /// ItemID로 Item ScriptableObject를 가져온다.
    /// Resources/Items 폴더 내의 아이템들을 대상으로 한다.
    /// </summary>
    private Item GetItemById(int id)
    {
        // 캐시가 없으면 한 번 빌드
        if (_itemCache == null)
        {
            _itemCache = new Dictionary<int, Item>();

            // Resources/Items 폴더에 있는 모든 Item 불러오기
            var allItems = Resources.LoadAll<Item>("Items"); // 폴더명이 "Resources/Items"라면 경로는 "Items"

            foreach (var it in allItems)
            {
                if (it == null) continue;

                // Item ScriptableObject에 ItemID 라는 필드가 있다고 가정
                if (!_itemCache.ContainsKey(it.ItemID))
                {
                    _itemCache.Add(it.ItemID, it);
                }
                else
                {
                    Debug.LogWarning($"[InventoryMain] Duplicate ItemID {it.ItemID} on item asset {it.name}.");
                }
            }
        }

        if (_itemCache.TryGetValue(id, out var found))
            return found;

        Debug.LogWarning($"[InventoryMain] No Item found for id={id}. Check ItemID or Resources/Items setup.");
        return null;
    }

    /// <summary>
    /// 아이디로 아이템 가져오기(성능상 남발은 지양 권장)
    /// </summary>
    public void AcquireItem(int id, int count = 1)
    {
        if (count <= 0) return;

        var item = GetItemById(id);
        if (item == null) return;

        // 이미 검증된 로직 재사용
        AcquireItem(item, count);
    }

}
