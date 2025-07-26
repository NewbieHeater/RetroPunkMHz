using System;
using UnityEngine;
[Serializable]
public class TransformCommands : Command
{
    public string instanceID;
    public Vector3 position;
    public Quaternion rotation;
    public TransformCommands(string id, Vector3 pos, Quaternion rot)
    {
        instanceID = id;
        position = pos;
        rotation = rot;
    }
    public override void Execute()
    {
        if (ReplayRegistry.TryGetInstance(instanceID, out var obj) && obj != null)
        {
            // 플레이어 기준 오프셋 적용
            var targetPos = position + Invoker.Instance.ReplayOffset;
            obj.transform.SetPositionAndRotation(targetPos, rotation);
        }
    }
}
