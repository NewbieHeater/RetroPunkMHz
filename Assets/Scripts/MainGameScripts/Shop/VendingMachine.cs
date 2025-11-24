using Game.Controls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : InteractableBase
{

    private bool _isOpen = false;
    protected override bool OnInteract()
    {

        // 이미 이벤트가 재생 중이면 중복 실행 방지
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
        // Interact만 허용 (나머지 공격/이동/점프 전부 차단)
        GlobalInputRouter.Instance.LockAllowOnly(GameInputAction.Interact);
        UIManagers.Instance.ShowUI();
    }

    private void CloseShop()
    {
        // 입력 잠금 해제
        GlobalInputRouter.Instance.Unlock();
        UIManagers.Instance.HideUI();
    }
}
