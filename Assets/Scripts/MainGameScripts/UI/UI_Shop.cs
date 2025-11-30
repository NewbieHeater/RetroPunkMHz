using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Shop : UI_Popup
{
    private enum GameObjects
    {
        GridPanel,
    }

    private GameObject gridPanel;
    private readonly List<UI_ShopSlot> _slots = new List<UI_ShopSlot>();

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));

        gridPanel = Get<GameObject>((int)GameObjects.GridPanel);

        // 프리팹에 미리 붙어 있던 자식들 정리
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);
    }

    /// <summary>
    /// 특정 ShopBase의 현재 판매 목록을 기반으로 UI 갱신.
    /// </summary>
    public void RefreshUI(ShopBase shop)
    {
        if (shop == null || shop.Items == null)
            return;

        var items = shop.Items;
        int needed = items.Count;

        // 1) 슬롯 개수 부족하면 추가 생성
        while (_slots.Count < needed)
        {
            var slot = Managers.UI.MakeSubItem<UI_ShopSlot>(gridPanel.transform);
            _slots.Add(slot);
        }

        // 2) 필요한 개수만큼 슬롯 세팅 + 활성화
        for (int i = 0; i < needed; i++)
        {
            var entry = items[i];
            var slot = _slots[i];

            if (!slot.gameObject.activeSelf)
                slot.gameObject.SetActive(true);

            // 슬롯에 Shop + Index만 넘김 (데이터는 항상 Shop에서 가져오게)
            slot.Setup(shop, i);
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
