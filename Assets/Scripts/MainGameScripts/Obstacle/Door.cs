using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Door : MonoBehaviour, IExplosionInteract
{
    [SerializeField] TextMeshProUGUI RequireText;

    public int RequiredAmpPts => 2;

    public int RequiredPerPts => 3;

    public int RequiredWavPts => 4;

    private void Start()
    {
        RequireText.text = $"{RequiredAmpPts} {RequiredPerPts} {RequiredWavPts}";
    }

    public void OnExplosionInteract(Channel channel)
    {
        if(channel.periodPoints == RequiredPerPts && channel.amplitudePoints == RequiredAmpPts && channel.waveformPoints == RequiredWavPts)
        {
            Destroy(gameObject);
        }
    }
}
