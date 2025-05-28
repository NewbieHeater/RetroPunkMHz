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

    // 각 슬라이더당 최고 포인트
    private int minPts;
    private int maxPts;
    private int prevAmp, prevPer, prevWav;

    // 최대 채널포인트
    private int maxChannelPoint;
    private int curChannelPoint;

    void Awake()
    {
        maxChannelPoint = ChannelManager.Instance.totalChannelPoints;
        curChannelPoint = maxChannelPoint;
        minPts = ChannelManager.Instance.minPts;
        maxPts = ChannelManager.Instance.maxPts;

        SliderSetting(amplitudeSlider);
        SliderSetting(periodSlider);
        SliderSetting(waveformSlider);
    }

    private void SliderSetting(Slider slider)
    {
        slider.wholeNumbers = true;
        slider.minValue = minPts;
        slider.maxValue = maxPts;

        slider.onValueChanged.AddListener((newValue) =>
            OnSliderChanged(newValue, slider)
        );
    }

    private void OnSliderChanged(float newValue, Slider changed)
    {
        int newVal = Mathf.RoundToInt(newValue);

        // 1) 이전 값
        int oldVal = changed == amplitudeSlider ? prevAmp
                   : changed == periodSlider ? prevPer
                                                : prevWav;

        // 2) 절대값 기준으로 더 많은 포인트를 쓰는지 판단
        int oldCost = Mathf.Abs(oldVal);
        int newCost = Mathf.Abs(newVal);
        bool usesMorePoints = newCost > oldCost;

        // 3) 다른 두 슬라이더의 현재 소모 합
        int costOthers = (changed == amplitudeSlider)
            ? Mathf.Abs(prevPer) + Mathf.Abs(prevWav)
            : (changed == periodSlider)
                ? Mathf.Abs(prevAmp) + Mathf.Abs(prevWav)
                : Mathf.Abs(prevAmp) + Mathf.Abs(prevPer);

        // 4) 만약 더 많은 포인트를 쓰려다가 한계를 넘으면 값 되돌리기
        if (usesMorePoints && newCost + costOthers > maxChannelPoint)
        {
            changed.value = oldVal;
            return;
        }

        // 5) 정상 변경: prev 업데이트
        if (changed == amplitudeSlider) prevAmp = newVal;
        else if (changed == periodSlider) prevPer = newVal;
        else prevWav = newVal;

        // 6) UI 갱신
        RefreshUI();
    }

    private void RefreshUI()
    {
        // 절대값 합산해서 남은 포인트 계산
        int totalUsed = Mathf.Abs(prevAmp) + Mathf.Abs(prevPer) + Mathf.Abs(prevWav);
        curChannelPoint = maxChannelPoint - totalUsed;

        channelPointText.text = $"남은 채널 포인트 : {curChannelPoint}";
        a.text = prevAmp.ToString();
        b.text = prevPer.ToString();
        c.text = prevWav.ToString();
    }




    public void OnSaveButtonClicked()
    {
        ChannelManager.Instance.Allocate(Mathf.RoundToInt(amplitudeSlider.value), Mathf.RoundToInt(periodSlider.value), Mathf.RoundToInt(waveformSlider.value));
    }
}
