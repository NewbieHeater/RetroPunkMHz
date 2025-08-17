using UnityEngine;

[CreateAssetMenu(fileName = "ReverbData", menuName = "Reverb/New Reverb Data")]
public class ReverbData : ScriptableObject
{
    public string id;
    public string title;
    public ReverbLine[] lines;
    public bool loop = false;
    public AudioReverbPreset reverbPreset = AudioReverbPreset.Off;
}
