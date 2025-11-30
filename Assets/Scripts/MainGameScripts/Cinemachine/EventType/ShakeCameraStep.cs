using UnityEngine;
using System.Collections;
public class ShakeCameraStep : EventStep
{
    [Header("Shake Settings")]
    [Min(0f)] public float duration = 0.15f;
    public float amplitude = 2.0f;
    public float frequency = 2.0f;
    [Min(0f)] public float fadeOut = 0.25f;

    [Header("Flow")]
    public bool waitForCompletion = true;

    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (ctx == null || ctx.CameraShake == null)
        {
            Debug.LogWarning("[ShakeCameraStep] CameraShakeNoise 가 설정되어 있지 않습니다.");
            yield break;
        }

        // 카메라 흔들기 시작
        ctx.CameraShake.ShakeOnce(duration, amplitude, frequency, fadeOut);

        // 그냥 쏘고 바로 다음 스텝으로 넘어가고 싶으면
        if (!waitForCompletion)
            yield break;

        float total = duration + fadeOut;
        float t = 0f;

        while (t < total)
        {
            if (ctx.IsCancelled != null && ctx.IsCancelled())
                yield break;

            t += Time.unscaledDeltaTime;   // ← 여기만 바꿔도 연출 길이는 고정됨
            yield return null;
        }
    }
}
