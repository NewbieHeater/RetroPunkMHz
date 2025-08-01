using UnityEngine;

/// <summary>
/// 여러 아이템을 담을 가장 기본적인 인벤토리
/// </summary>
public class InventoryMain : InventoryBase
{
    public static bool IsInventoryActive = false;  // 인벤토리 활성화 되었는가?
    
    new void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        TryOpenInventory();
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
        mInventoryBase.SetActive(true);
        IsInventoryActive = true;

        //커서 활성화
        UnlockCursor();
    }

    /// <summary>
    /// 인벤토리를 닫는다.
    /// </summary>
    public void CloseInventory()
    {
        mInventoryBase.SetActive(false);
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
        return null;
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
            for (int i = 0; i < mSlots.Length; i++)
            {
                //마스크를 사용하여 해당 슬롯이 마스크에 허용되는 위치인경우에만 아이템을 집어넣도록 한다.
                if (mSlots[i].Item != null && mSlots[i].IsMask(item))
                {
                    if (mSlots[i].Item.ItemID == item.ItemID)
                    {
                        //현재 슬롯의 아이템 개수(Count)를 갱신한다.
                        mSlots[i].UpdateSlotCount(count);
                        return;
                    }
                }
            }
        }

        //장비 아이템이 아닌경우 새로운 슬롯에 놓는다.
        for (int i = 0; i < mSlots.Length; i++)
        {
            if (mSlots[i].Item == null && mSlots[i].IsMask(item))
            {
                mSlots[i].AddItem(item, count);
                return;
            }
        }
    }
}