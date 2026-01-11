using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 여러 아이템을 담을 가장 기본적인 인벤토리
/// </summary>
public class InventoryMain : Singleton<InventoryMain>
{

    public static bool IsInventoryActive = false;

    [SerializeField] protected GameObject _inventoryBase; // Inventory 최상위 부모(활성/비활성 목적)
    [SerializeField] protected GameObject _inventorySlotsParent;  // Slot들을 담을 부모 게임오브젝트
    [SerializeField] protected InventorySlot[] _slots;
    /// <summary>
    /// 인벤토리 베이스를 초기화 시킨다.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (_inventoryBase.activeSelf)
        {
            _inventoryBase.SetActive(false);
        }

        _slots = _inventorySlotsParent.GetComponentsInChildren<InventorySlot>();
    }
    void Update()
    {
        TryOpenInventory();
    }

    public List<ItemData> savedItems = new List<ItemData>();

    // 슬롯 정보를 저장
    public void SaveFromSlots()
    {
        savedItems.Clear();

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].Item != null)
            {
                savedItems.Add(
                    new ItemData(i, _slots[i].Item, _slots[i].ItemCount)
                );
            }
        }
    }

    // 씬이 로드될 때 슬롯에 다시 정보를 넣는 용도 or UI교체
    public void LoadToSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].ClearSlot();
        }

        foreach (var data in savedItems)
        {
            if (data.slotIndex < _slots.Length)
            {
                _slots[data.slotIndex].AddItem(data.item, data.count);
            }
        }
    }


    /// <summary>
    /// 인벤토리를 I키를 눌러 열거나 닫는다.
    /// </summary>
    private void TryOpenInventory()
    {
        //옵션이 켜져있는경우 비활성화
        if (GameMenuManager.IsOptionActive) { return; }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!IsInventoryActive)
                OpenInventory();
            else
                CloseInventory();
        }
    }

    /// <summary>
    /// 인벤토리를 연다.
    /// </summary>
    private void OpenInventory()
    {
        _inventoryBase.SetActive(true);
        IsInventoryActive = true;

        //커서 활성화
        UnlockCursor();
    }

    /// <summary>
    /// 인벤토리를 닫는다.
    /// </summary>
    public void CloseInventory()
    {
        _inventoryBase.SetActive(false);
        IsInventoryActive = false;

        //커서 비활성화
        TryLockCursor();
    }
    public void TryLockCursor()
    {

    }
    public void UnlockCursor()
    {

    }
    public InventorySlot[] GetAllItems()
    {
        return _slots;
    }

    /// <summary>
    /// 특정 아이템 슬롯에 아이템을 등록시킨다
    /// </summary>
    /// <param name="item">어떤 아이템?</param>
    /// <param name="targetSlot">어느 슬롯에?</param>
    /// <param name="count">개수는?></param>
    public void AcquireItem(Item item, InventorySlot targetSlot, int count = 1)
    {
        //중첩이 가능하다면?
        if (item.CanOverlap)
        {
            //마스크를 사용하여 해당 슬롯이 마스크에 허용되는 위치인경우에만 아이템을 집어넣도록 한다.
            if (targetSlot.Item != null && targetSlot.IsMask(item))
            {
                if (targetSlot.Item.ItemID == item.ItemID)
                {
                    //현재 슬롯의 아이템 개수(Count)를 갱신한다.
                    targetSlot.UpdateSlotCount(count);
                }
            }
        }
        else
        {
            targetSlot.AddItem(item, count);
        }


    }


    public void AcquireItem(Item item, int count = 1)
    {
        //중첩이 가능하다면?
        if (item.CanOverlap)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                //마스크를 사용하여 해당 슬롯이 마스크에 허용되는 위치인경우에만 아이템을 집어넣도록 한다.
                if (_slots[i].Item != null && _slots[i].IsMask(item))
                {
                    if (_slots[i].Item.ItemID == item.ItemID)
                    {
                        //현재 슬롯의 아이템 개수(Count)를 갱신한다.
                        _slots[i].UpdateSlotCount(count);
                        return;
                    }
                }
            }
        }

        //장비 아이템이 아닌경우 새로운 슬롯에 놓는다.
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].Item == null && _slots[i].IsMask(item))
            {
                _slots[i].AddItem(item, count);
                return;
            }
        }
    }

}