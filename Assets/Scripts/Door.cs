using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Door : Enemy
{
    public TextMeshProUGUI textMeshProUGUI;

    private void Awake()
    {
        textMeshProUGUI.text = $"{requiredAmpPts} {requiredPerPts} {requiredWavPts}";
    }

    public override void OnExplosionInteract(Channel channel)
    {
        if (channel.amplitudePoints >= requiredAmpPts &&
            channel.periodPoints >= requiredPerPts &&
            channel.waveformPoints >= requiredWavPts)
        {
            Destroy(gameObject);
        }
    }
}
