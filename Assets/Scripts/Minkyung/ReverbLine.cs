using UnityEngine;

[System.Serializable]
public class ReverbLine
{
    public string speaker;
    [TextArea(2, 6)]
    public string text;
    public AudioClip audioClip;
    public float delayBefore;
    public float overrideDuration;
}
