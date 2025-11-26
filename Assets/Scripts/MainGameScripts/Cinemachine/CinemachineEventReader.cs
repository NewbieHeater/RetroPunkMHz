using Game.Controls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineEventReader : Singleton<CinemachineEventReader>
{
    [Header("Refs")]
    [SerializeField] private CinemachineFocusing focusing;
    [SerializeField] private DialogueManager dialogueManager;
    CinemachineEventContext _currentCtx;
    public CinemachineEventAsset defaultDialogueSequence;

    private readonly Queue<QueuedEvent> _queue = new();

    private Coroutine _runner;
    private bool _cancelRequested;
    private bool _running;

    public bool IsRunning => _running;

    // 외부 트리거
    public void EndEvent()
    {
        _currentCtx?.EndFlag?.Invoke();
    }

    public void ResetToBaseCam()
    {
        if (focusing != null) focusing.ToDefault();
    }

    public void PlayEvent(CinemachineEventAsset asset)
    {
        // 대사 파라미터 없이 호출하는 기존 방식
        PlayEvent(asset, null, null);
    }

    public void CancelCurrent()
    {
        _cancelRequested = true;
    }

    public struct QueuedEvent
    {
        public CinemachineEventAsset asset;
        public string dialogueFile;
        public string dialogueGroup;
    }

    

    public void PlayEvent(CinemachineEventAsset asset, string dialogueFile, string dialogueGroup)
    {
        if (asset == null) return;

        _queue.Enqueue(new QueuedEvent
        {
            asset = asset,
            dialogueFile = dialogueFile,
            dialogueGroup = dialogueGroup
        });

        if (_runner == null)
            _runner = StartCoroutine(RunQueue());
    }

    public void PlayDialogueSequence(string fileName, string groupName)
    {
        PlayEvent(defaultDialogueSequence, fileName, groupName);
    }

    private IEnumerator RunQueue()
    {
        while (_queue.Count > 0)
        {
            var qe = _queue.Dequeue();
            yield return RunEvent(qe);
        }
        _runner = null;
    }

    private IEnumerator RunEvent(QueuedEvent qe)
    {
        var asset = qe.asset;
        _running = true;
        _cancelRequested = false;

        _currentCtx = new CinemachineEventContext
        {
            Focusing = focusing,
            SavedCameraSlot = GetCurrentCameraSlotSafe(),
            IsCancelled = () => _cancelRequested,
            LockInput = (on) => GlobalInputRouter.Instance?.LockInput(on),

            DialogueManager = dialogueManager,
            DialogueFileName = qe.dialogueFile,
            DialogueGroupName = qe.dialogueGroup
        };

        if (asset.lockPlayerInputWhileRunning)
            _currentCtx.LockInput?.Invoke(true);

        try
        {
            foreach (var step in asset.steps)
            {
                if (step == null || _cancelRequested) break;
                yield return step.Execute(_currentCtx);
            }

            if (asset.postDelay > 0f)
                yield return new WaitForSeconds(asset.postDelay);
        }
        finally
        {
            if (asset.lockPlayerInputWhileRunning)
                _currentCtx.LockInput?.Invoke(false);

            _currentCtx = null;
            _running = false;
        }
    }

    private int GetCurrentCameraSlotSafe()
    {
        // Focusing에 질의 API 있으면 사용하세요.
        return focusing ? focusing.CurrentSlotOrDefault() : 0;
    }
}
