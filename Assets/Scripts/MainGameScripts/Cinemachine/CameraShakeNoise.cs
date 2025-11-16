using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraShakeNoise : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CinemachineVirtualCamera vCam;

    [Header("Default")]
    [SerializeField] private float defaultFadeOut = 0.25f;

    [Header("Charge Kill Preset")]
    [SerializeField] private float chargeKillDuration = 0.15f;
    [SerializeField] private float chargeKillAmp = 2.0f;
    [SerializeField] private float chargeKillFreq = 2.0f;

    CinemachineBasicMultiChannelPerlin perlin;
    float baseAmp;
    float baseFreq;

    Coroutine shakeRoutine;

    private void Awake()
    {
        if (vCam == null)
            vCam = GetComponent<CinemachineVirtualCamera>();

        if (vCam != null)
        {
            perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlin != null)
            {
                baseAmp = perlin.m_AmplitudeGain;
                baseFreq = perlin.m_FrequencyGain;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShakeOnChargeKill();
        }
    }

    IEnumerator ShakeCoroutine(float duration, float amp, float freq, float fadeOut)
    {
        if (perlin == null)
            yield break;

        // 흔들기 시작: 기본값 + 오프셋
        float targetAmp = baseAmp + amp;
        float targetFreq = baseFreq + freq;

        perlin.m_AmplitudeGain = targetAmp;
        perlin.m_FrequencyGain = targetFreq;

        // 강하게 유지
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 페이드 아웃
        float startAmp = perlin.m_AmplitudeGain;
        float startFreq = perlin.m_FrequencyGain;

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            float lerp = t / fadeOut; // 0 → 1

            perlin.m_AmplitudeGain = Mathf.Lerp(startAmp, baseAmp, lerp);
            perlin.m_FrequencyGain = Mathf.Lerp(startFreq, baseFreq, lerp);

            yield return null;
        }

        perlin.m_AmplitudeGain = baseAmp;
        perlin.m_FrequencyGain = baseFreq;
        shakeRoutine = null;
    }

    /// <summary>
    /// 일반 용도: 원하는 세기/시간으로 카메라를 한 번 흔들기
    /// </summary>
    public void ShakeOnce(float duration, float amp, float freq, float fadeOut = -1f)
    {
        if (perlin == null)
            return;

        if (fadeOut < 0f)
            fadeOut = defaultFadeOut;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeCoroutine(duration, amp, freq, fadeOut));
    }

    /// <summary>
    /// 차지 공격으로 적 처치 시 호출할 프리셋
    /// </summary>
    public void ShakeOnChargeKill()
    {
        ShakeOnce(chargeKillDuration, chargeKillAmp, chargeKillFreq);
    }
}
