using UnityEngine;

/// <summary>
/// ���� �������� ���� ���� �⺻���� �κ��丮
/// </summary>
public class InventoryMain : InventoryBase
{
    public static bool IsInventoryActive = false;  // �κ��丮 Ȱ��ȭ �Ǿ��°�?
    
    new void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        TryOpenInventory();
    }

    /// <summary>
    /// �κ��丮�� IŰ�� ���� ���ų� �ݴ´�.
    /// </summary>
    private void TryOpenInventory()
    {
        //�ɼ��� �����ִ°�� ��Ȱ��ȭ
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
    /// �κ��丮�� ����.
    /// </summary>
    private void OpenInventory()
    {
        mInventoryBase.SetActive(true);
        IsInventoryActive = true;

        //Ŀ�� Ȱ��ȭ
        UnlockCursor();
    }

    /// <summary>
    /// �κ��丮�� �ݴ´�.
    /// </summary>
    public void CloseInventory()
    {
        mInventoryBase.SetActive(false);
        IsInventoryActive = false;

        //Ŀ�� ��Ȱ��ȭ
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
    /// Ư�� ������ ���Կ� �������� ��Ͻ�Ų��
    /// </summary>
    /// <param name="item">� ������?</param>
    /// <param name="targetSlot">��� ���Կ�?</param>
    /// <param name="count">������?></param>
    public void AcquireItem(Item item, InventorySlot targetSlot, int count = 1)
    {
        //��ø�� �����ϴٸ�?
        if (item.CanOverlap)
        {
            //����ũ�� ����Ͽ� �ش� ������ ����ũ�� ���Ǵ� ��ġ�ΰ�쿡�� �������� ����ֵ��� �Ѵ�.
            if (targetSlot.Item != null && targetSlot.IsMask(item))
            {
                if (targetSlot.Item.ItemID == item.ItemID)
                {
                    //���� ������ ������ ����(Count)�� �����Ѵ�.
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
        //��ø�� �����ϴٸ�?
        if (item.CanOverlap)
        {
            for (int i = 0; i < mSlots.Length; i++)
            {
                //����ũ�� ����Ͽ� �ش� ������ ����ũ�� ���Ǵ� ��ġ�ΰ�쿡�� �������� ����ֵ��� �Ѵ�.
                if (mSlots[i].Item != null && mSlots[i].IsMask(item))
                {
                    if (mSlots[i].Item.ItemID == item.ItemID)
                    {
                        //���� ������ ������ ����(Count)�� �����Ѵ�.
                        mSlots[i].UpdateSlotCount(count);
                        return;
                    }
                }
            }
        }

        //��� �������� �ƴѰ�� ���ο� ���Կ� ���´�.
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