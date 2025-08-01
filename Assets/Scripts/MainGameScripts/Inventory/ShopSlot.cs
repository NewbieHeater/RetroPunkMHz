using Unity.VisualScripting;
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
    public Item Item => mItem;

    [Header("아이템의 이미지를 할당할 오브젝트")]
    [SerializeField] private Image mItemImage;

    [Header("버튼")]
    [SerializeField] private Button mButton;


    private void Awake()
    {
        if (mInventory == null)
        {
            Debug.LogError("[ShopSlot] InventoryMain이 할당되지 않았습니다.");
            mInventory = GameObject.Find("InventoryManager").GetComponent<InventoryMain>();
        }    
        if (mItemImage == null)
        {
            Debug.LogError("[ShopSlot] ItemImage가 할당되지 않았습니다.");
            mItemImage = GetComponentInChildren<Image>();
        }
        if (mButton == null)
        {
            Debug.LogError("[ShopSlot] Button이 할당되지 않았습니다.");
            mButton = GetComponentInChildren<Button>();
        }

        mItemImage.sprite = mItem.Image;
        mButton.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// 버튼 클릭시 행동
    /// </summary>
    public void OnButtonClicked()
    {
        if (IsItemAquireAble())
        {
            mInventory.AcquireItem(mItem);
        }
        else
        {
            //인벤토리에 아이템을 못넣는 상황에 할일
        }
    }

    // 사실 이미 AcquireItem() 에서 검사하고 있지만 한번더
    /// <summary>
    /// 인벤토리에 들어갈 수 있는지 검사
    /// </summary>
    private bool IsItemAquireAble()
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

        //이게 true이면 아이템이 인벤토리의 어떤 슬롯에도 들어갈 수 없는상태
        if (count == allitems.Length) { return false; }

        return true;
    }
}