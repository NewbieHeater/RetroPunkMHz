using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WaitNode : BTNode
{
    private readonly float _seconds;
    private readonly bool _unscaled; // 일시정지 중에도 흐르게 할지
    private float _deadline;

    public WaitNode(float seconds, bool unscaled = false, string name = null) : base(name)
    {
        _seconds = Mathf.Max(0f, seconds);
        _unscaled = unscaled;
    }

    protected override void OnStart(BTContext ctx)
    {
        // ctx에 시간 소스가 없다면 DeltaTime 누적 버전 유지
        float now = _unscaled ? ctx.UnscaledTime : ctx.Time; // 없다면 추가 권장
        _deadline = now + _seconds;
    }

    protected override NodeStatus OnUpdate(BTContext ctx)
    {
        float now = _unscaled ? ctx.UnscaledTime : ctx.Time;
        return now >= _deadline ? NodeStatus.Success : NodeStatus.Running;
    }
}
