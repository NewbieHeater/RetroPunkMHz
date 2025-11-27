using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC : MonoBehaviour
{



    // Update is called once per frame


    public void pc_amp_multiply()
    {
        int amp = ChannelManager.AmpPts;
        int per = ChannelManager.PerPts;
        int wav = ChannelManager.WavPts;
        int pc_channel = 2 * amp;
        Debug.Log("神神");
        ChannelManager.Instance.Allocate(pc_channel, per, wav);
    }

    public void pc_per_multiply()
    {
        int amp = ChannelManager.AmpPts;
        int per = ChannelManager.PerPts;
        int wav = ChannelManager.WavPts;
        int pc_channel = 2 * per;
        Debug.Log("神神");
        ChannelManager.Instance.Allocate(amp, pc_channel, wav);
    }

    public void pc_wav_multiply()
    {
        int amp = ChannelManager.AmpPts;
        int per = ChannelManager.PerPts;
        int wav = ChannelManager.WavPts;
        int pc_channel = 2 * wav;
        Debug.Log("神神");
        ChannelManager.Instance.Allocate(amp, per, pc_channel);
    }
}
