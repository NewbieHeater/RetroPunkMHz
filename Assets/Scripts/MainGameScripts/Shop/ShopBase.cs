using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuyableItems
{
    public Item item;
    public int itemCount;
}


public class ShopBase : InteractableBase
{
    [SerializeField]
    private BuyableItems[] buyableItems;

    /// <summary>
    /// 상점이 가진 판매 목록(읽기 전용)을 외부에 노출.
    /// </summary>
    public IReadOnlyList<BuyableItems> Items => buyableItems;

    /// <summary>
    /// 슬롯 인덱스 기준으로 구매 시도.
    /// 성공 시 해당 아이템을 out으로 돌려준다.
    /// </summary>
    public bool TryBuy(int index, out Item boughtItem)
    {
        boughtItem = null;

        if (buyableItems == null) return false;
        if (index < 0 || index >= buyableItems.Length) return false;

        var entry = buyableItems[index];
        if (entry == null || entry.item == null) return false;

        if (entry.itemCount <= 0)
            return false;

        // TODO: 골드/코스트 체크가 생기면 여기서 처리 후 실패 시 false 반환

        entry.itemCount--;
        boughtItem = entry.item;
        return true;
    }

    /// <summary>현 재고 수량 조회용 헬퍼.</summary>
    public int GetStock(int index)
    {
        if (buyableItems == null || index < 0 || index >= buyableItems.Length)
            return 0;

        var entry = buyableItems[index];
        return entry != null ? entry.itemCount : 0;
    }
}
