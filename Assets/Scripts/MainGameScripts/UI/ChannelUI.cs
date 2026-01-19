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

    void Start()
    {
        maxChannelPoint = ChannelManager.Instance.totalChannelPoints;
        curChannelPoint = maxChannelPoint;
        minPts = ChannelManager.Instance.minPts;
        maxPts = ChannelManager.Instance.maxPts;

        SliderSetting(amplitudeSlider);
        SliderSetting(periodSlider);
        SliderSetting(waveformSlider);

        RefreshUI();
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

        int oldVal = changed == amplitudeSlider ? prevAmp
                   : changed == periodSlider ? prevPer
                                                : prevWav;

        // 현재 다른 슬라이더들의 실제 "순 소모치" (양수는 소모, 음수는 회수)
        int othersTotal = prevAmp + prevPer + prevWav;

        // 현재 슬라이더의 이전값 빼고 (해당 슬라이더 새값만 더할 예정)
        if (changed == amplitudeSlider) othersTotal -= prevAmp;
        else if (changed == periodSlider) othersTotal -= prevPer;
        else othersTotal -= prevWav;

        // 새로운 총합
        int newTotal = othersTotal + newVal;

        // 남은 포인트 계산 (양수 초과 시 제한)
        int used = Mathf.Clamp(newTotal, -maxChannelPoint, maxChannelPoint);
        int remaining = maxChannelPoint - used;

        // 한도를 초과할 경우 막기
        if (newTotal > maxChannelPoint)
        {
            changed.value = oldVal;
            return;
        }

        if (changed == amplitudeSlider) prevAmp = newVal;
        else if (changed == periodSlider) prevPer = newVal;
        else prevWav = newVal;

        RefreshUI();
    }

    private void RefreshUI()
    {
        int totalUsed = prevAmp + prevPer + prevWav; // 부호 포함
        curChannelPoint = maxChannelPoint - totalUsed;

        // 초과 방지
        curChannelPoint = Mathf.Clamp(curChannelPoint, 0, maxChannelPoint * 2);


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
