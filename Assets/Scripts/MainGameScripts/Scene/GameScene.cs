using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.GameScene;

        
    }

    public override void Clear()
    {
        InventoryMain.Instance.SaveFromSlots();
        InventoryMain.Instance.CloseInventory();
    }
}
