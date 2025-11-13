using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineEventReader : Singleton<CinemachineEventReader>
{
    [Header("Refs")]
    [SerializeField] private CinemachineFocusing focusing;

    private readonly Queue<CinemachineEventAsset> _queue = new();
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
        if (asset == null) return;
        _queue.Enqueue(asset);
        if (_runner == null) _runner = StartCoroutine(RunQueue());
    }

    public void CancelCurrent()
    {
        _cancelRequested = true;
    }

    CinemachineEventContext _currentCtx;

    private IEnumerator RunQueue()
    {
        while (_queue.Count > 0)
        {
            var asset = _queue.Dequeue();
            yield return RunEvent(asset);
        }
        _runner = null;
    }

    private IEnumerator RunEvent(CinemachineEventAsset asset)
    {
        _running = true; _cancelRequested = false;

        // 컨텍스트 구성
        _currentCtx = new CinemachineEventContext
        {
            Focusing = focusing,
            SavedCameraSlot = GetCurrentCameraSlotSafe(),
            IsCancelled = () => _cancelRequested,
            LockInput = (on) => GlobalInputRouter.Instance?.LockInput(on)
        };

        // 입력 잠금
        if (asset.lockPlayerInputWhileRunning)
            _currentCtx.LockInput?.Invoke(true);

        // 실행
        try
        {
            foreach (var step in asset.steps)
            {
                if (step == null || _cancelRequested) break;
                yield return step.Execute(_currentCtx);
            }
            if (asset.postDelay > 0f) yield return new WaitForSeconds(asset.postDelay);
        }
        finally
        {
            // 입력 잠금 해제
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
