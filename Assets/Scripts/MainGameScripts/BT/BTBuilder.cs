using Decorators;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BTBuilder
{
    private readonly Stack<Composite> _stack = new Stack<Composite>();
    private BTNode _root;

    private void Add(BTNode node)
    {
        if (_stack.Count > 0) _stack.Peek().AddChild(node);
        else
        {
            if (_root != null) throw new InvalidOperationException("루트 노드는 하나만 가능합니다.");
            _root = node;
        }
    }

    public BTBuilder Sequence(string name = null) { var n = new Sequence(name); Add(n); _stack.Push(n); return this; }
    public BTBuilder Selector(string name = null) { var n = new Selector(name); Add(n); _stack.Push(n); return this; }
    public BTBuilder Parallel(Parallel.Policy success, Parallel.Policy failure, string name = null)
    { var n = new Parallel(success, failure, name); Add(n); _stack.Push(n); return this; }

    public BTBuilder End()
    {
        if (_stack.Count == 0) throw new InvalidOperationException("End를 호출할 Composite가 없습니다.");
        _stack.Pop();
        return this;
    }

    public BTBuilder Do(string name, Func<BTContext, NodeStatus> onTick, Action<BTContext> onStart = null, Action<BTContext, NodeStatus> onStop = null)
    { Add(new ActionNode(name, onTick, onStart, onStop)); return this; }

    public BTBuilder Wait(float seconds, string name = null) { Add(new WaitNode(seconds, name ?? $"Wait({seconds})")); return this; }

    public BTBuilder Guard(Func<BTContext, bool> predicate, Action<BTBuilder> body, string name = null)
    {
        // Guard는 Composite를 하나 감싸는 형태로 구현
        var inner = new Sequence("GuardInner");
        var guard = new Guard(predicate, inner, name ?? "Guard");
        Add(guard);
        _stack.Push(inner);
        body(this);
        End(); // inner
        return this;
    }

    public BTBuilder Inverter(Action<BTBuilder> body, string name = null)
    {
        var inner = new Sequence("InverterInner");
        var inv = new Inverter(inner, name ?? "Inverter");
        Add(inv);
        _stack.Push(inner);
        body(this);
        End();
        return this;
    }

    public BTBuilder Succeeder(Action<BTBuilder> body, string name = null)
    {
        var inner = new Sequence("SucceederInner");
        var suc = new Succeeder(inner, name ?? "Succeeder");
        Add(suc);
        _stack.Push(inner);
        body(this);
        End();
        return this;
    }

    public BTBuilder Cooldown(float seconds, bool failWhileCooling, Action<BTBuilder> body, string name = null)
    {
        var inner = new Sequence("CooldownInner");
        var cd = new Cooldown(inner, seconds, failWhileCooling, name ?? $"Cooldown({seconds})");
        Add(cd);
        _stack.Push(inner);
        body(this);
        End();
        return this;
    }

    public BTNode Build()
    {
        if (_stack.Count != 0) throw new InvalidOperationException("모든 Composite는 End()로 닫혀야 합니다.");
        if (_root == null) throw new InvalidOperationException("루트 노드가 없습니다.");
        return _root;
    }
}