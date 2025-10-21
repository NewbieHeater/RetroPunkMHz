using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineEventReader : Singleton<CinemachineEventReader>
{
    [Header("Refs")]
    [SerializeField] private CinemachineFocusing focusing;

    private bool running;
    private bool endFlag;
    private int savedCameraSlot = -1;

    public void EndEvent() => endFlag = false;
    public void ResetToBaseCam() => endFlag = false;
    public bool IsRunning => running;

    public void PlayEvent(CinemachineEventAsset asset)
    {
        if (running || asset == null) return;
        StartCoroutine(RunEvent(asset));
    }

    private IEnumerator RunEvent(CinemachineEventAsset asset)
    {
        running = true;
        endFlag = true;

        if (asset.lockPlayerInputWhileRunning)
            GlobalInputRouter.Instance.LockInput(true);

        savedCameraSlot = GetCurrentCameraSlotSafe();

        // 단일 디스패처: 스위치는 여기만!
        foreach (var step in asset.steps)
        {
            switch (step.type)
            {
                case StepType.LockInput:
                    GlobalInputRouter.Instance.LockInput(step.lockOn);
                    break;

                case StepType.CameraFocus:
                    FocusToSafe(step.cameraSlot);
                    break;

                case StepType.Dialogue:
                    yield return DialogueManager.Instance.StartDialogueAndWait(step.fileName, step.groupName);
                    break;

                case StepType.WaitEndFlag:
                    endFlag = true;
                    yield return new WaitUntil(() => endFlag == false);
                    break;

                case StepType.Delay:
                    if (step.seconds > 0f) yield return new WaitForSeconds(step.seconds);
                    break;

                case StepType.RestoreCamera:
                    if (focusing != null) focusing.ToDefault();
                    break;
            }
        }

        if (asset.postDelay > 0f)
            yield return new WaitForSeconds(asset.postDelay);

        if (asset.lockPlayerInputWhileRunning)
            GlobalInputRouter.Instance.LockInput(false);

        running = false;
    }

    private void FocusToSafe(int slot)
    {
        if (focusing != null) focusing.FocusTo(slot);
        else Debug.LogWarning("[CinemachineEventReader] No focusing assigned.");
    }

    private int GetCurrentCameraSlotSafe()
    {
        // focusing에 질의 API가 없으면 임시로 0
        return 0;
    }
}
