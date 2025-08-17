using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WaitNode : BTNode
{
    private readonly float _seconds;
    private float _remain;

    public WaitNode(float seconds, string name = null) : base(name) { _seconds = seconds; }

    protected override void OnStart(BTContext ctx) { _remain = _seconds; }

    protected override NodeStatus OnUpdate(BTContext ctx)
    {
        _remain -= ctx.DeltaTime;
        return _remain <= 0f ? NodeStatus.Success : NodeStatus.Running;
    }
}