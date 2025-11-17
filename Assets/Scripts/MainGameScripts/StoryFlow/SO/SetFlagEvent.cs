// Scripts/Story/Events/SetFlagEvent.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Events/Set Flag")]
public class SetFlagEvent : GameEventSO
{
    public string key;
    public bool value = true;

    public override void Invoke()
    {
        GameProgress.I.SetFlag(key, value);
    }
}
