using Game.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuiltInEvents
{
    ChargeKill,
    Dialogue,
    ShakeCameraWithSlowMotion,
    ShakeCamera,
}

public class CinemachineEventReader : Singleton<CinemachineEventReader>
{
    [Header("Refs")]
    [SerializeField] private CinemachineFocusing focusing;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private CameraShakeNoise cameraShaker;
    [SerializeField] private CinemachineEventAsset[] builtIns;

    CinemachineEventContext _currentCtx;

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
#if UNITY_EDITOR
    private void OnValidate()
    {
        // enum 개수만큼 배열 길이 자동 맞추기
        int count = Enum.GetValues(typeof(BuiltInEvents)).Length;
        if (builtIns == null || builtIns.Length != count)
        {
            Array.Resize(ref builtIns, count);
        }
    }
#endif
    private CinemachineEventAsset GetBuiltInAsset(BuiltInEvents id)
    {
        int idx = (int)id;
        if (builtIns == null || idx < 0 || idx >= builtIns.Length)
        {
            Debug.LogWarning($"[CinemachineEventReader] Built-in array가 초기화되지 않았습니다. ({id})");
            return null;
        }

        var asset = builtIns[idx];
        if (asset == null)
        {
            Debug.LogWarning($"[CinemachineEventReader] {id} 슬롯에 에셋이 할당되지 않았습니다.");
        }

        return asset;
    }

    public void PlayBuiltInEvent(BuiltInEvents builtIn)
    {
        var asset = GetBuiltInAsset(builtIn);
        if (asset == null) return;

        // 대화 파라미터 없는 일반 이벤트
        PlayEvent(asset, null, null);
    }

    // 빌트인 + 대사 이름을 동시에 넘길 수 있는 버전 (Dialogue용)
    public void PlayBuiltInEvent(BuiltInEvents builtIn, string dialogueFile, string dialogueGroup)
    {
        var asset = GetBuiltInAsset(builtIn);
        if (asset == null) return;

        PlayEvent(asset, dialogueFile, dialogueGroup);
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
            DialogueGroupName = qe.dialogueGroup,

            CameraShake = cameraShaker
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
