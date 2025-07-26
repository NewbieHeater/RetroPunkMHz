using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 베이스로써 인벤토리 슬롯들을 등록시키고 사용할 준비를 완료한다.
/// 추상클래스로 작성하여 인벤토리 베이스 자체적으로 인스턴스 할 수 없게 한다.
/// </summary>
abstract public class InventoryBase : MonoBehaviour
{
    [SerializeField] protected GameObject mInventoryBase; // Inventory 최상위 부모(활성/비활성 목적)
    [SerializeField] protected GameObject mInventorySlotsParent;  // Slot들을 담을 부모 게임오브젝트
    [SerializeField] protected InventorySlot[] mSlots;
    /// <summary>
    /// 인벤토리 베이스를 초기화 시킨다.
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