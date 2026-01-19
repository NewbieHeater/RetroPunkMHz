using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopSlot : UI_Base, IPointerClickHandler
{
    private enum GameObjects
    {
        ItemImage,
        ItemCount
    }

    private Image _itemImage;
    private TextMeshProUGUI _itemCountText;

    private ShopBase _shop;
    private int _index;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));

        _itemImage = Get<GameObject>((int)GameObjects.ItemImage).GetComponent<Image>();
        _itemCountText = Get<GameObject>((int)GameObjects.ItemCount).GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 이 슬롯이 어떤 Shop의 몇 번째 아이템을 표시할지 지정.
    /// </summary>
    public void Setup(ShopBase shop, int index)
    {
        _shop = shop;
        _index = index;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_shop == null) return;

        var items = _shop.Items;
        if (_index < 0 || _index >= items.Count) return;

        var entry = items[_index];
        if (entry == null || entry.item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_itemImage != null)
            _itemImage.sprite = entry.item.Image;

        if (_itemCountText != null)
            _itemCountText.text = entry.itemCount.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_shop == null) return;

        // 실제 구매 로직은 ShopBase가 담당
        if (_shop.TryBuy(_index, out var boughtItem))
        {
            // 인벤토리에 추가
            if (boughtItem != null)
                InventoryManager.Instance.AddItem(boughtItem);

            // 재고 변경 반영
            RefreshUI();
        }
        else
        {
            Debug.Log("구매 실패: 재고 없음 또는 조건 불충족");
        }
    }
}
