using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{

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
        //if (mInventory == null)
        //{
        //    Debug.LogError("[ShopSlot] InventoryMain이 할당되지 않았습니다.");
        //    mInventory = GameObject.Find("InventoryManager").GetComponent<InventoryMain>();
        //}    
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
        InventoryManager.Instance.AddItem(mItem);
    }

    // 사실 이미 AcquireItem() 에서 검사하고 있지만 한번더
    /// <summary>
    /// 인벤토리에 들어갈 수 있는지 검사
    /// </summary>

}