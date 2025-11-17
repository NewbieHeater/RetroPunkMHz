// Scripts/Story/Events/RemoveEntityEvent.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Events/Remove Entity")]
public class RemoveEntityEvent : GameEventSO
{
    public string entityGuid;

    public override void Invoke()
    {
        if (!string.IsNullOrEmpty(entityGuid))
            GameProgress.I.RemoveEntity(entityGuid);
    }
}
