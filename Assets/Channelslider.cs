using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChannelSliderController : MonoBehaviour
{
    public Slider ChannelSlider;
    public TMP_Text ChannelValueText;

    private void Start()
    {
        ChannelSlider.onValueChanged.AddListener(UpdateVolumeText);
        UpdateVolumeText(ChannelSlider.value);
    }

    private void UpdateVolumeText(float value)
    {
        ChannelValueText.text = Mathf.RoundToInt(value).ToString();
    }
}