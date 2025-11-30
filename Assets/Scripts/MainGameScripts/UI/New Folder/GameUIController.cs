using Game.Controls;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    private UI_Inven _openedInventory;   // 현재 열려 있는 인벤토리 팝업

    private void Update()
    {
        var input = GlobalInputRouter.Instance.CurrentFrame;

        if (input.buttons.IsDown(GameInputAction.InventoryToggle))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        // 1) 이미 열려 있다면 → 닫기
        if (_openedInventory != null)
        {
            Managers.UI.ClosePopupUI(_openedInventory);
            _openedInventory = null;
            return;
        }

        // 2) 닫혀 있다면 → 열기
        _openedInventory = Managers.UI.ShowPopupUI<UI_Inven>();
        _openedInventory.RefreshAll();
    }
}
