using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBase : InteractableBase
{
    public BuyableItems[] buyableItems;

    public bool TryBuy(Item item)
    {
        // buyableItems 배열에서 해당 아이템을 찾는다
        for (int i = 0; i < buyableItems.Length; i++)
        {
            if (buyableItems[i].item == item)
            {
                if (buyableItems[i].itemCount > 0)
                {
                    buyableItems[i].itemCount--;
                    return true;
                }
                return false;
            }
        }
        return false;
    }
}
