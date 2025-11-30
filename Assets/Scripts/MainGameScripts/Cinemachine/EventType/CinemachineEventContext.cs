using System;
using System.Collections;
using UnityEngine;

public class CinemachineEventContext
{
    public CinemachineFocusing Focusing;
    public int SavedCameraSlot;
    public System.Func<bool> IsCancelled;
    public System.Action<bool> LockInput;
    public System.Action EndFlag;

    public string DialogueFileName;
    public string DialogueGroupName;

    public DialogueManager DialogueManager;

    public CameraShakeNoise CameraShake;
}


public abstract class EventStep : ScriptableObject
{
    public abstract IEnumerator Execute(CinemachineEventContext ctx);
}

public enum EventStepType
{
    LockInput,
    UnlockInput,
    Dialogue,
    CameraSlot,
    Wait
}