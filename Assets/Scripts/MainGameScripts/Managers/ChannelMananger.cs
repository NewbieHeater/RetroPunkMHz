using UnityEngine;

public class ChannelManager : MonoBehaviour
{
    public static ChannelManager Instance { get; private set; }

    [Header("총 채널 포인트")]
    public int totalChannelPoints = 12;

    [Header("속성별 최소/최대값")]
    public int minPts = -5;
    public int maxPts = 5;

    [Header("현재 할당된 포인트")]
    [SerializeField] int amplitudePts = 0;
    [SerializeField] int periodPts = 0;
    [SerializeField] int waveformPts = 0;

    public Channel CurrentChannel { get; private set; }

    // ─── 여기에 추가 ───
    // 정적 편의 프로퍼티
    public static int AmpPts => Instance.amplitudePts;
    public static int PerPts => Instance.periodPts;
    public static int WavPts => Instance.waveformPts;
    public static Channel Current => Instance.CurrentChannel;

    // 조건 검사 메서드
    public static bool Meets(int reqAmp, int reqPer, int reqWav)
    {
        return AmpPts >= reqAmp
            && PerPts >= reqPer
            && WavPts >= reqWav;
    }
    // ───────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetAllocation();
    }

    public void ResetAllocation()
    {
        amplitudePts = periodPts = waveformPts = 0;
        UpdateChannel();
    }

    public bool Allocate(int newAmp, int newPer, int newWav)
    {
        if (newAmp < minPts || newAmp > maxPts) return false;
        if (newPer < minPts || newPer > maxPts) return false;
        if (newWav < minPts || newWav > maxPts) return false;

        int used = Mathf.Max(0, newAmp)
                 + Mathf.Max(0, newPer)
                 + Mathf.Max(0, newWav);
        if (used > totalChannelPoints) return false;

        amplitudePts = newAmp;
        periodPts = newPer;
        waveformPts = newWav;
        Debug.Log("allo");
        UpdateChannel();
        return true;
    }

    private void UpdateChannel()
    {
        CurrentChannel = new Channel(amplitudePts, periodPts, waveformPts);
    }
}

public struct Channel
{
    // 할당된 포인트 값
    public int amplitudePoints;
    public int periodPoints;
    public int waveformPoints;

    public Channel(int a, int p, int w)
    {
        amplitudePoints = a;
        periodPoints = p;
        waveformPoints = w;
    }
}