using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.RoomScene;


    }

    public override void Clear()
    {
        InventoryMain.Instance.SaveFromSlots();
        InventoryMain.Instance.CloseInventory();
    }
}
