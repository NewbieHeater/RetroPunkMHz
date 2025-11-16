using System.Collections;
using UnityEngine;

public sealed class CinemachineEventContext
{
    public CinemachineFocusing Focusing;
    public System.Func<bool> IsCancelled;   // 취소 체크
    public System.Action<bool> LockInput;   // 입력 잠금
    public System.Action EndFlag;           // 외부 트리거
    public int SavedCameraSlot;
}

public abstract class EventStep : ScriptableObject
{
    public abstract IEnumerator Execute(CinemachineEventContext ctx);
}

