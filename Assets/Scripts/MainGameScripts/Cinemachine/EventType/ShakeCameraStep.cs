using System.Collections;
using UnityEngine;

public class ShakeCameraStep : EventStep
{
    [Header("Shake Settings")]
    [Min(0f)] public float duration = 0.15f;     // 강하게 흔드는 시간
    public float amplitude = 2.0f;
    public float frequency = 2.0f;
    [Min(0f)] public float fadeOut = 0.25f;      // 서서히 원래 값으로 복귀

    [Header("Flow")]
    public bool waitForCompletion = true;        // true면 흔들림이 끝날 때까지 다음 스텝 대기

    public override IEnumerator Execute(CinemachineEventContext ctx)
    {
        if (ctx == null || ctx.CameraShake == null)
        {
            Debug.LogWarning("[ShakeCameraStep] CameraShakeNoise 가 설정되어 있지 않습니다.");
            yield break;
        }

        // 흔들기 시작
        ctx.CameraShake.ShakeOnce(duration, amplitude, frequency, fadeOut);

        // 그냥 쏘고 바로 다음 스텝으로 넘어가고 싶으면
        if (!waitForCompletion)
            yield break;

        // duration + fadeOut 동안 대기 (취소 대응)
        float total = duration + fadeOut;
        float t = 0f;

        while (t < total)
        {
            if (ctx.IsCancelled != null && ctx.IsCancelled())
                yield break;

            t += Time.deltaTime;   // 연출 시간은 게임 시간 기준으로 진행
            yield return null;
        }
    }
}
