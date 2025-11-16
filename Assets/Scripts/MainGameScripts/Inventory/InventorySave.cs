using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySave : MonoBehaviour
{
    // Start is called before the first frame update
    public static InventorySave Instance;
    public List<ItemSlotData> savedSlots = new List<ItemSlotData>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 넘어가도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SaveInventory(InventorySlot[] slots)
    {
        savedSlots.Clear();
        foreach (var slot in slots)
        {
            if (slot.Item != null)
                savedSlots.Add(new ItemSlotData(slot.Item.ItemID, slot.ItemCount));
        }
    }

    // 슬롯 로드
    public List<ItemSlotData> LoadInventory()
    {
        return savedSlots;
    }

    [System.Serializable]
    public class ItemSlotData
    {
        public int itemID;
        public int count;
        public ItemSlotData(int id, int c)
        {
            itemID = id;
            count = c;
        }
    }

}
