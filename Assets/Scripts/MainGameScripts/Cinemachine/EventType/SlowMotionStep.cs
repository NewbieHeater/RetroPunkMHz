using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Cinemachine Event/Slow Motion Step")]
public class SlowMotionStep : EventStep
{
    [Header("Target TimeScale")]

    /// <summary>
    /// 슬로우 모션에서 사용할 목표 Time.timeScale 값입니다.
    /// 일반 속도는 1.0이며, 값이 낮을수록 더 느려집니다.
    /// 예) 0.2 → 20% 속도로 느려짐
    /// </summary>
    [Tooltip("슬로우 모션 상태에서 Time.timeScale이 도달할 목표 값 (낮을수록 더 느려짐).")]
    [Range(0.01f, 1f)]
    public float targetTimeScale = 0.2f;


    [Header("Durations (Realtime 기준)")]

    /// <summary>
    /// Time.timeScale이 원래 값 → targetTimeScale으로 부드럽게 전환되는 데 걸리는 시간입니다.
    /// Time.unscaledDeltaTime 기준으로 진행됩니다.
    /// </summary>
    [Tooltip("기존 속도에서 targetTimeScale까지 전환하는 데 걸리는 시간 (부드러운 슬로우 인).")]
    [Min(0f)]
    public float blendInDuration = 0.1f;

    /// <summary>
    /// 슬로우 모션 상태(targetTimeScale)에서 유지되는 시간입니다.
    /// Time.unscaledDeltaTime 기준으로 측정됩니다.
    /// </summary>
    [Tooltip("슬로우 모션 상태를 유지하는 시간 (Realtime 기준).")]
    [Min(0f)]
    public float holdDuration = 0.5f;

    /// <summary>
    /// Time.timeScale이 다시 원래 값으로 되돌아오는 데 걸리는 시간입니다.
    /// blend-in과 동일하게 Time.unscaledDeltaTime 기준으로 실행됩니다.
    /// </summary>
    [Tooltip("targetTimeScale 상태에서 원래 속도로 되돌아가는 데 걸리는 시간 (부드러운 슬로우 아웃).")]
    [Min(0f)]
    public float blendOutDuration = 0.2f;


    [Header("Options")]

    /// <summary>
    /// true일 경우, 슬로우 모션 종료 후 Time.timeScale을 반드시 원래 값으로 복구합니다.
    /// false이면 이벤트 이후에도 슬로우 상태가 유지됩니다.
    /// </summary>
    [Tooltip("슬로우 모션 종료 후 Time.timeScale을 원래 값으로 강제로 복구할지 여부.")]
    public bool restoreTimeScale = true;

    /// <summary>
    /// true일 경우, Time.fixedDeltaTime도 함께 조정하여 물리(Physics) 업데이트 속도도 슬로우 모션에 맞춥니다.
    /// false면 물리 시간은 그대로 유지됩니다.
    /// </summary>
    [Tooltip("Time.fixedDeltaTime도 함께 줄여서 물리 시스템까지 슬로우 모션을 적용할지 여부.")]
    public bool affectFixedDeltaTime = true;



    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        float originalScale = Time.timeScale;
        float originalFixedDelta = Time.fixedDeltaTime;

        // --------------- Blend In (정상 → 슬로우) ---------------
        if (blendInDuration > 0f)
        {
            float t = 0f;
            while (t < blendInDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / blendInDuration);
                float scale = Mathf.Lerp(originalScale, targetTimeScale, k);
                Time.timeScale = scale;

                if (affectFixedDeltaTime)
                    Time.fixedDeltaTime = originalFixedDelta * scale;

                if (ctx.IsCancelled != null && ctx.IsCancelled())
                    yield break;

                yield return null;
            }
        }
        else
        {
            Time.timeScale = targetTimeScale;

            if (affectFixedDeltaTime)
                Time.fixedDeltaTime = originalFixedDelta * targetTimeScale;
        }

        // -------------------- Hold (슬로우 유지) --------------------
        if (holdDuration > 0f)
        {
            float t = 0f;
            while (t < holdDuration)
            {
                t += Time.unscaledDeltaTime;

                if (ctx.IsCancelled != null && ctx.IsCancelled())
                    break;

                yield return null;
            }
        }

        // -------------------- Blend Out (슬로우 → 정상) --------------------
        if (restoreTimeScale)
        {
            if (blendOutDuration > 0f)
            {
                float startScale = Time.timeScale;
                float t = 0f;
                while (t < blendOutDuration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / blendOutDuration);
                    float scale = Mathf.Lerp(startScale, originalScale, k);
                    Time.timeScale = scale;

                    if (affectFixedDeltaTime)
                        Time.fixedDeltaTime = originalFixedDelta * scale;

                    if (ctx.IsCancelled != null && ctx.IsCancelled())
                        break;

                    yield return null;
                }
            }

            // 무조건 원래값으로 맞춰 안전하게 복귀
            Time.timeScale = originalScale;
            if (affectFixedDeltaTime)
                Time.fixedDeltaTime = originalFixedDelta;
        }
    }
}
