using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ActionNode : BTNode
{
    private readonly Action<BTContext> _onStart;
    private readonly Func<BTContext, NodeStatus> _onTick;
    private readonly Action<BTContext, NodeStatus> _onStop;

    public ActionNode(
        string name,
        Func<BTContext, NodeStatus> onTick,
        Action<BTContext> onStart = null,
        Action<BTContext, NodeStatus> onStop = null) : base(name)
    {
        _onStart = onStart;
        _onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));
        _onStop = onStop;
    }

    protected override void OnStart(BTContext ctx) { _onStart?.Invoke(ctx); }
    protected override NodeStatus OnUpdate(BTContext ctx) { return _onTick(ctx); }
    protected override void OnStop(BTContext ctx) { _onStop?.Invoke(ctx, Status); }
}
