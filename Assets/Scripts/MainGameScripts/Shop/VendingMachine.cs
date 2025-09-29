using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : InteractableBase
{
    protected override bool OnInteract()
    {

        // 이미 이벤트가 재생 중이면 중복 실행 방지
        if (CinemachineEventReader.Instance != null && CinemachineEventReader.Instance.IsRunning)
            return false;

        UIMangers.Instance.TogleUI();

        return true;
        

    }
}
