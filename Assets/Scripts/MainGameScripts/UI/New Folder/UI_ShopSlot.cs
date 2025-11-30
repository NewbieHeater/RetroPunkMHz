using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopSlot : UI_Base, IPointerClickHandler
{
    enum GameObjects
    {
        ItemImage,
        ItemCount
    }

    public Item curItem;
    private Image ItemImage;
    private TextMeshProUGUI ItemCountText;
    private int ItemCount;

    ShopBase shop;

    void Awake()
    {
        Init();
    }

    public override void Init()
    {

        Bind<GameObject>(typeof(GameObjects));

        ItemImage = Get<GameObject>((int)GameObjects.ItemImage).GetComponent<Image>();
        ItemCountText = Get<GameObject>((int)GameObjects.ItemCount).GetComponent<TextMeshProUGUI>();
               
    }

    public void Setup(ShopBase vm, Item item, int count)
    {
        shop = vm;
        curItem = item;
        ItemCount = count;

        RefreshUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool success = shop.TryBuy(curItem);

        InventoryManager.Instance.AddItem(curItem);

        ItemCount--;
        ItemCountText.text = $"{ItemCount}";
    }

    public void RefreshUI()
    {
        ItemImage.sprite = curItem.Image;
        ItemCountText.text = $"{ItemCount}";
    }


    public void SetItem(Item item)
    {
        curItem = item;
    }
    public void SetItemCount(int count)
    {
        ItemCount = count;
    }
}
