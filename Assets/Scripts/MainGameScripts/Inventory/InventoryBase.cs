using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �κ��丮 ���̽��ν� �κ��丮 ���Ե��� ��Ͻ�Ű�� ����� �غ� �Ϸ��Ѵ�.
/// �߻�Ŭ������ �ۼ��Ͽ� �κ��丮 ���̽� ��ü������ �ν��Ͻ� �� �� ���� �Ѵ�.
/// </summary>
abstract public class InventoryBase : MonoBehaviour
{
    [SerializeField] protected GameObject _inventoryBase; // Inventory �ֻ��� �θ�(Ȱ��/��Ȱ�� ����)
    [SerializeField] protected GameObject _inventorySlotsParent;  // Slot���� ���� �θ� ���ӿ�����Ʈ
    [SerializeField] protected InventorySlot[] _slots;
    /// <summary>
    /// �κ��丮 ���̽��� �ʱ�ȭ ��Ų��.
    /// </summary>
    protected void Awake()
    {
        if (_inventoryBase.activeSelf)
        {
            _inventoryBase.SetActive(false);
        }
        
        _slots = _inventorySlotsParent.GetComponentsInChildren<InventorySlot>();
    }
}