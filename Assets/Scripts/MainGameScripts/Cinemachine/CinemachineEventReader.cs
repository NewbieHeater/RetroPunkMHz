using System.Collections;
using UnityEngine;

public class CinemachineEventReader : Singleton<CinemachineEventReader>
{
    [Header("Refs")]
    [SerializeField] private CinemachineFocusing focusing; // FocusTo(int) 보유 스크립트


    private bool running;
    private int savedCameraSlot = -1;
    private bool eventEnd;
    public void EndEvent()
    {
        eventEnd = false;
    }

    public bool IsRunning => running;

    public void PlayEvent(CinemachineEventAsset asset)
    {
        if (running) return;
        StartCoroutine(RunEvent(asset));
    }

    private IEnumerator RunEvent(CinemachineEventAsset asset)
    {
        running = true;
        eventEnd = true;
        if (asset.lockPlayerInputWhileRunning)
            GlobalInputRouter.Instance.LockInput(true);

        savedCameraSlot = GetCurrentCameraSlotSafe();
        ApplyBlendHint(asset.blendHint);

        switch (asset.mode)
        {
            case CinemachineEventAsset.EventMode.CameraAndDialogue:
                FocusToSafe(asset.cameraSlot);
                yield return DialogueManager.Instance.StartDialogueAndWait(asset.fileName, asset.groupName);
                break;

            case CinemachineEventAsset.EventMode.DialogueOnly:
                // 카메라는 유지
                yield return DialogueManager.Instance.StartDialogueAndWait(asset.fileName, asset.groupName);
                break;

            case CinemachineEventAsset.EventMode.CameraOnly:
                FocusToSafe(asset.cameraSlot);
                yield return new WaitUntil(() => !eventEnd);
                break;
        }

        if (asset.restoreCameraAfter && savedCameraSlot >= 0)
            if (focusing != null) focusing.ToDefault();

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

    public void ResetToBaseCam()
    {
        eventEnd = false;    
    }

    private int GetCurrentCameraSlotSafe()
    {
        // focusing에 현재 슬롯을 질의할 API가 없다면 캐시를 관리하거나 0으로 가정
        return 0;
    }

    private void ApplyBlendHint(CinemachineEventAsset.BlendHint hint)
    {
        // focusing 또는 Cinemachine Brain Blend 세팅에 힌트를 전달하는 훅
        // 필요 시 구현
    }
}