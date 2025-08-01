using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private InventoryMain mInventory;


    [Header("�ش� ������Ʈ�� �Ҵ�Ǵ� ������")]
    [SerializeField] private Item mItem;
    /// <summary>
    /// ��ȣ�ۿ� ������ ��ü�� ������ �ִ� ������
    /// /// </summary>
    /// <value></value>
    public Item Item
    {
        get
        {
            return mItem;
        }
    }

    [Header("�������� �̹����� �Ҵ�")]
    [SerializeField] private Image mItemImage;

    public void OnButtonClicked()
    {
        InventorySlot[] allitems = mInventory.GetAllItems();

        int count = 0;
        for (; count < allitems.Length; ++count)
        {
            //���� ������ ĭ�� null�̶�� �ֿ� �� �ִ� ����
            if (allitems[count].Item == null) { break; }

            //���� ������ĭ�� null�� �ƴ�����, ���� �����۰� �����ϸ鼭 ��ø�� ������ �������̶�� �ֿ� �� �ִ� ����
            if (allitems[count].Item.ItemID == mItem.ItemID && allitems[count].Item.CanOverlap) { break; }
        }

    }
}