using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStateMachine<T>
{
    private IState<T> _currentState;
    private T _owner;

    public DataStateMachine(T owner, IState<T> initial)
    {
        _owner = owner;
        _currentState = initial;
        _currentState.OperateEnter(_owner);
    }

    public void ChangeState(IState<T> next)
    {
        _currentState.OperateExit(_owner);
        _currentState = next;
        _currentState.OperateEnter(_owner);
        Debug.Log(next.ToString());
    }

    public void Update() => _currentState.OperateUpdate(_owner);
    public void FixedUpdate() => _currentState.OperateFixedUpdate(_owner);
}