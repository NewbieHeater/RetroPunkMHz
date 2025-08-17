using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BTNode
{
    public string Name { get; private set; }
    public NodeStatus Status { get; private set; } = NodeStatus.Failure;

    private bool _started;

    protected BTNode(string name = null) { Name = name ?? GetType().Name; }

    public NodeStatus Tick(BTContext ctx)
    {
        if (!_started)
        {
            _started = true;
            OnStart(ctx);
        }

        Status = OnUpdate(ctx);

        if (Status != NodeStatus.Running)
        {
            OnStop(ctx);
            _started = false;
        }
        return Status;
    }

    public void Abort(BTContext ctx)
    {
        if (_started)
        {
            OnStop(ctx);
            _started = false;
        }
        Status = NodeStatus.Failure;
    }

    public virtual void Reset(BTContext ctx) => Abort(ctx);

    protected virtual void OnStart(BTContext ctx) { }
    protected abstract NodeStatus OnUpdate(BTContext ctx);
    protected virtual void OnStop(BTContext ctx) { }
}