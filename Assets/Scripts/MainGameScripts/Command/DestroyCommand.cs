using System;
using UnityEngine;

[Serializable]
public class DestroyCommand : Command
{
    public string instanceID;

    public DestroyCommand(string instanceID)
    {
        this.instanceID = instanceID;
    }

    public override void Execute()
    {
        if (ReplayRegistry.TryGetInstance(instanceID, out var obj) && obj != null)
            GameObject.Destroy(obj);
    }
}
