using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Player
    public Vector3 playerPosition;
    public int playerHp;

    // Inventory
    public List<ItemData> inventoryItems;

    //Scene
    public string sceneName;
}
