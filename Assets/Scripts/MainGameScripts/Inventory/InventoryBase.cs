using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �κ��丮 ���̽��ν� �κ��丮 ���Ե��� ��Ͻ�Ű�� ����� �غ� �Ϸ��Ѵ�.
/// �߻�Ŭ������ �ۼ��Ͽ� �κ��丮 ���̽� ��ü������ �ν��Ͻ� �� �� ���� �Ѵ�.
/// </summary>
abstract public class InventoryBase : MonoBehaviour
{
    [SerializeField] protected GameObject mInventoryBase; // Inventory �ֻ��� �θ�(Ȱ��/��Ȱ�� ����)
    [SerializeField] protected GameObject mInventorySlotsParent;  // Slot���� ���� �θ� ���ӿ�����Ʈ
    [SerializeField] protected InventorySlot[] mSlots;
    /// <summary>
    /// �κ��丮 ���̽��� �ʱ�ȭ ��Ų��.
    /// </summary>
    protected void Awake()
    {
        if (mInventoryBase.activeSelf)
        {
            mInventoryBase.SetActive(false);
        }

        mSlots = mInventorySlotsParent.GetComponentsInChildren<InventorySlot>();
    }
}