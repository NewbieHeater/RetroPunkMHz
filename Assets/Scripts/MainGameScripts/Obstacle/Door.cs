using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IExplosionInteract
{
    public int RequiredAmpPts => 0;

    public int RequiredPerPts => 0;

    public int RequiredWavPts => 0;

    public void OnExplosionInteract(Channel channel)
    {
        if(channel.periodPoints == RequiredPerPts && channel.amplitudePoints == RequiredAmpPts && channel.waveformPoints == RequiredWavPts)
        {
            Destroy(gameObject);
        }
    }
}
