using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CineEventContext
{
    public MonoBehaviour runner;                 // StartCoroutine용 (보통 EventReader 자신)
    public CinemachineFocusing focusing;         // 카메라 제어
    public int savedCameraSlot = -1;
    public bool isRunning;
    public bool endFlag;                         // 외부에서 EndEvent()로 false로 바꿈
    public Action<bool> setInputLocked;          // 입력 잠금 훅
    public Func<int> getCurrentCamSlot;          // 현재 슬롯 조회 훅
}
