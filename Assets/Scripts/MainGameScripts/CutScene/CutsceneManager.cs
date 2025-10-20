using System;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [SerializeField] private CutsceneUI ui;

    private CutsceneAsset current;
    private int stepIdx = -1;
    private int frameIdx = -1;
    private int expIdx = -1;
    private Action onFinished;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!ui) ui = FindObjectOfType<CutsceneUI>();
        ui?.Show(false);
    }

    /// <summary>
    /// 컷씬 재생 시작
    /// </summary>
    public void Play(CutsceneAsset asset, Action onFinished = null)
    {
        if (asset == null || asset.steps == null || asset.steps.Count == 0)
        {
            Debug.LogWarning("[Cutscene] Invalid asset.");
            return;
        }

        current = asset;
        this.onFinished = onFinished;

        IsPlaying = true;
        stepIdx = -1; frameIdx = -1; expIdx = -1;

        ui.Show(true);
        NextStep();
    }

    /// <summary>
    /// 컷씬 완전 종료
    /// </summary>
    public void StopAll()
    {
        IsPlaying = false;
        current = null;
        stepIdx = frameIdx = expIdx = -1;

        if (ui)
        {
            ui.ClearExplanation();
            ui.Show(false);
        }

        var cb = onFinished;
        onFinished = null;
        cb?.Invoke();
    }

    private void NextStep()
    {
        stepIdx++;
        frameIdx = -1;
        expIdx = -1;

        if (current == null || stepIdx >= current.steps.Count)
        {
            StopAll();
            return;
        }

        var step = current.steps[stepIdx];
        ui.SetBackground(step.backgroundColor);

        NextFrame();
    }

    private void NextFrame()
    {
        frameIdx++;
        expIdx = -1;

        var step = current.steps[stepIdx];
        if (step.frames == null || frameIdx >= step.frames.Count)
        {
            NextStep();
            return;
        }

        var frame = step.frames[frameIdx];

        // 배경 오버라이드
        if (frame.overrideBackground)
            ui.SetBackground(frame.backgroundColor);

        // 가운데 이미지
        ui.SetCenter(frame.centerSprite, frame.centerSize);

        // 첫 설명 출력
        ProceedExplanation();
    }

    private void ProceedExplanation()
    {
        var frame = current.steps[stepIdx].frames[frameIdx];

        expIdx++;
        bool hasExp = frame.explanations != null && frame.explanations.Count > 0;

        if (!hasExp || expIdx >= frame.explanations.Count)
        {
            // 다음 프레임으로
            NextFrame();
            return;
        }

        string text = frame.explanations[expIdx];
        ui.ShowExplanation(text, frame.useTyping, frame.typingDelay);
    }

    private void SkipFrame()
    {
        var frame = current.steps[stepIdx].frames[frameIdx];
        if (!frame.allowSkipFrame) return;

        NextFrame();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        var frame = current.steps[stepIdx].frames[frameIdx];

        // 좌클릭/Enter → 다음 설명 (타이핑 중이면 먼저 완성)
        // CutsceneManager.Update() 안의 좌클릭/Enter 처리 부분만 교체
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
        {

            if (ui.IsTyping())
            {
                if (frame.allowSkipTyping)
                {
                    ui.CompleteTyping();   // 글자 즉시 완성
                }
                else
                {
                    // 스킵 불가 상태면 입력 무시 (다음 설명으로 진행 금지)
                    return;
                }
            }
            else
            {
                // 타이핑이 이미 끝난 상태일 때만 다음 설명으로
                ProceedExplanation();
            }
        }


        // 우클릭/Space → 현재 프레임 스킵
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space))
        {
            SkipFrame();
        }

        // Esc → 전체 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopAll();
        }
    }
}
