using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ChannelUI : MonoBehaviour
{
    public Slider amplitudeSlider;
    public Slider periodSlider;
    public Slider waveformSlider;

    public Text channelPointText;

    public TextMeshProUGUI a;
    public TextMeshProUGUI b;
    public TextMeshProUGUI c;

    public int minPts;
    public int maxPts;

    void Awake()
    {
        maxPoint = ChannelManager.Instance.totalChannelPoints;
        curPoint = maxPoint;
        minPts = ChannelManager.Instance.minPts;
        maxPts = ChannelManager.Instance.maxPts;

        amplitudeSlider.wholeNumbers = true;
        periodSlider.wholeNumbers = true;
        waveformSlider.wholeNumbers = true;

        amplitudeSlider.minValue = minPts;
        periodSlider.minValue = minPts;
        waveformSlider.minValue = minPts;

        amplitudeSlider.maxValue = maxPts;
        periodSlider.maxValue = maxPts;
        waveformSlider.maxValue = maxPts;

        amplitudeSlider.onValueChanged.AddListener(OnSliderChanged);
        periodSlider.onValueChanged.AddListener(OnSliderChanged);
        waveformSlider.onValueChanged.AddListener(OnSliderChanged);

        OnSliderChanged(amplitudeSlider.value);
    }
    int maxPoint;
    int curPoint;
    private void OnSliderChanged(float newValue)
    {
        curPoint = maxPoint - Mathf.RoundToInt(Mathf.Abs(amplitudeSlider.value) + Mathf.Abs(periodSlider.value) + Mathf.Abs(waveformSlider.value));
        channelPointText.text = $"남은 채널 포인트 : {curPoint}";

        a.text = $"{amplitudeSlider.value}";
        b.text = $"{periodSlider.value}";
        c.text = $"{waveformSlider.value}";
    }

    public void OnSaveButtonClicked()
    {
        ChannelManager.Instance.Allocate(Mathf.RoundToInt(amplitudeSlider.value), Mathf.RoundToInt(periodSlider.value), Mathf.RoundToInt(waveformSlider.value));
    }

    
}
