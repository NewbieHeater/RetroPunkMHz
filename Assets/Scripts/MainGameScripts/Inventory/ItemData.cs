using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int slotIndex;
    public Item item;
    public int count;

    public ItemData(int slotIndex, Item item, int count)
    {
        this.slotIndex = slotIndex;
        this.item = item;
        this.count = count;
    }
}
