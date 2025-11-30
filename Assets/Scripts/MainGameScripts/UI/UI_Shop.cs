using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Shop : UI_Popup
{
    enum GameObjects
    {
        GridPanel,
    }

    public GameObject gridPanel;
    private List<UI_ShopSlot> _slots = new List<UI_ShopSlot>();
    private BuyableItems[] _buyableItems;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));

        gridPanel = Get<GameObject>((int)GameObjects.GridPanel);
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);
    }

    public void RefreshUI(BuyableItems[] buyableItems, ShopBase shop)
    {
        if (buyableItems == null)
            return;

        int needed = buyableItems.Length;

        // 1) 슬롯 개수 부족하면 추가 생성
        while (_slots.Count < needed)
        {
            var slot = Managers.UI.MakeSubItem<UI_ShopSlot>(gridPanel.transform);
            _slots.Add(slot);
        }

        // 2) 필요한 개수만큼 슬롯 채우고 활성화
        for (int i = 0; i < needed; i++)
        {
            var data = buyableItems[i];
            var slot = _slots[i];

            slot.Setup(shop, data.item, data.itemCount);

            if (!slot.gameObject.activeSelf)
                slot.gameObject.SetActive(true);

            slot.SetItem(data.item);
            slot.SetItemCount(data.itemCount);
            slot.RefreshUI();
        }

        // 3) 남는 슬롯들은 비활성화
        for (int i = needed; i < _slots.Count; i++)
        {
            var slot = _slots[i];
            if (slot != null && slot.gameObject.activeSelf)
                slot.gameObject.SetActive(false);
        }
    }

}
