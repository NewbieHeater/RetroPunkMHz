using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;

    public Item GetItemByID(int id)
    {
        return items.Find(x => x.ItemID == id);
    }
}

