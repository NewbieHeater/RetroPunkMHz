using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Door : Enemy
{
    public TextMeshProUGUI textMeshProUGUI;

    private void Awake()
    {
        textMeshProUGUI.text = $"{RequiredAmpPts} {RequiredPerPts} {RequiredWavPts}";
    }

    public override void OnExplosionInteract(Channel channel)
    {
        if (channel.amplitudePoints >= RequiredAmpPts &&
            channel.periodPoints >= RequiredPerPts &&
            channel.waveformPoints >= RequiredWavPts)
        {
            Destroy(gameObject);
        }
    }
}
