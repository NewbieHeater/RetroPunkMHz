using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private InventoryMain mInventory;


    [Header("해당 오브젝트에 할당되는 아이템")]
    [SerializeField] private Item mItem;
    /// <summary>
    /// 상호작용 가능한 객체가 가지고 있는 아이템
    /// /// </summary>
    /// <value></value>
    public Item Item
    {
        get
        {
            return mItem;
        }
    }

    [Header("아이템의 이미지를 할당")]
    [SerializeField] private Image mItemImage;

    public void OnButtonClicked()
    {
        InventorySlot[] allitems = mInventory.GetAllItems();

        int count = 0;
        for (; count < allitems.Length; ++count)
        {
            //현재 아이템 칸이 null이라면 주울 수 있는 상태
            if (allitems[count].Item == null) { break; }

            //현재 아이템칸이 null이 아니지만, 현재 아이템과 동일하면서 중첩이 가능한 아이템이라면 주울 수 있는 상태
            if (allitems[count].Item.ItemID == mItem.ItemID && allitems[count].Item.CanOverlap) { break; }
        }

    }
}