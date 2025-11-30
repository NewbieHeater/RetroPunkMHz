using Game.Controls;
using UnityEngine;

public class VendingMachine : ShopBase
{
    private UI_Shop shop;
    private bool _isOpen = false;

    protected override bool OnInteract()
    {
        // 시네머신 이벤트 재생 중이면 상호작용 막기
        if (CinemachineEventReader.Instance != null && CinemachineEventReader.Instance.IsRunning)
            return false;

        _isOpen = !_isOpen;

        if (_isOpen)
            OpenShop();
        else
            CloseShop();

        return true;
    }

    private void OpenShop()
    {
        // Interact, InventoryToggle만 허용
        GlobalInputRouter.Instance.LockAllowOnly(GameInputAction.Interact, GameInputAction.InventoryToggle);

        shop = Managers.UI.ShowPopupUI<UI_Shop>();
        shop.RefreshUI(this);
    }

    private void CloseShop()
    {
        GlobalInputRouter.Instance.Unlock();

        if (shop != null)
        {
            Managers.UI.ClosePopupUI(shop);
            shop = null;
        }
    }
}
