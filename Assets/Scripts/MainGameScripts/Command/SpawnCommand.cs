using System;
using UnityEngine;

[Serializable]
public class SpawnCommand : Command
{
    public string instanceID;
    public string prefabPath;
    public Vector3 position;
    public Quaternion rotation;

    public SpawnCommand(string instanceID, string prefabPath, Vector3 position, Quaternion rotation)
    {
        this.instanceID = instanceID;
        this.prefabPath = prefabPath;
        this.position = position;
        this.rotation = rotation;
    }

    public override void Execute()
    {
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"SpawnCommand: Prefab not found at Resources/{prefabPath}");
            return;
        }

        Vector3 spawnPos = position + Invoker.Instance.ReplayOffset;
        var obj = GameObject.Instantiate(prefab, spawnPos, rotation);
        var rec = obj.GetComponent<Recordable>();
        if (rec != null)
        {
            // ³ìÈ­ ½ÃÁ¡ÀÇ ID·Î µ¤¾î¾²±â
            rec.prefabPath = prefabPath;
            rec.InstanceID = instanceID;
        }
        obj.layer = 12;
        ReplayRegistry.RegisterInstance(instanceID, obj);
    }
}
