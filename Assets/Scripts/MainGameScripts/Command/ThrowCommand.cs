using System;
using UnityEngine;

[Serializable]
public class ThrowCommand : Command
{
    public string instanceID;
    public Vector3 launchForce;
    public ForceMode forceMode;

    public ThrowCommand(string instanceID, Vector3 launchForce, ForceMode forceMode = ForceMode.Impulse)
    {
        this.instanceID = instanceID;
        this.launchForce = launchForce;
        this.forceMode = forceMode;
    }

    public override void Execute()
    {
        if (ReplayRegistry.TryGetInstance(instanceID, out var obj) && obj != null)
        {
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(launchForce, forceMode);
        }
    }
}
