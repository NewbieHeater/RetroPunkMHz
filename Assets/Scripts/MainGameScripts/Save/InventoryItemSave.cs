using UnityEngine;
using System.Collections.Generic;

public class InventoryItemSave : MonoBehaviour, ISaveable
{
    InventoryMain inventory;

    private void Awake()
    {
        inventory = InventoryMain.Instance;
    }

    public void SaveData(SaveData data)
    {
        Debug.Log("InventoryItemSave.SaveData() CALLED");
        inventory.SaveFromSlots();

        // 그대로 복사 (타입 동일)
        data.inventoryItems = new List<ItemData>(
            inventory.savedItems
        );
        Debug.Log("Saved item count = " + data.inventoryItems.Count);
    }

    public void LoadData(SaveData data)
    {
        if (data.inventoryItems == null) return;

        inventory.savedItems = new List<ItemData>(
            data.inventoryItems
        );

        inventory.LoadToSlots();
    }
}