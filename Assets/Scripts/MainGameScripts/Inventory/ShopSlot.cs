using Unity.VisualScripting;
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
    public Item Item => mItem;

    [Header("�������� �̹����� �Ҵ��� ������Ʈ")]
    [SerializeField] private Image mItemImage;

    [Header("��ư")]
    [SerializeField] private Button mButton;


    private void Awake()
    {
        if (mInventory == null)
        {
            Debug.LogError("[ShopSlot] InventoryMain�� �Ҵ���� �ʾҽ��ϴ�.");
            mInventory = GameObject.Find("InventoryManager").GetComponent<InventoryMain>();
        }    
        if (mItemImage == null)
        {
            Debug.LogError("[ShopSlot] ItemImage�� �Ҵ���� �ʾҽ��ϴ�.");
            mItemImage = GetComponentInChildren<Image>();
        }
        if (mButton == null)
        {
            Debug.LogError("[ShopSlot] Button�� �Ҵ���� �ʾҽ��ϴ�.");
            mButton = GetComponentInChildren<Button>();
        }

        mItemImage.sprite = mItem.Image;
        mButton.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// ��ư Ŭ���� �ൿ
    /// </summary>
    public void OnButtonClicked()
    {
        if (IsItemAquireAble())
        {
            mInventory.AcquireItem(mItem);
        }
        else
        {
            //�κ��丮�� �������� ���ִ� ��Ȳ�� ����
        }
    }

    // ��� �̹� AcquireItem() ���� �˻��ϰ� ������ �ѹ���
    /// <summary>
    /// �κ��丮�� �� �� �ִ��� �˻�
    /// </summary>
    private bool IsItemAquireAble()
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

        //�̰� true�̸� �������� �κ��丮�� � ���Կ��� �� �� ���»���
        if (count == allitems.Length) { return false; }

        return true;
    }
}