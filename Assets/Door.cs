using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Enemy
{
    

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
