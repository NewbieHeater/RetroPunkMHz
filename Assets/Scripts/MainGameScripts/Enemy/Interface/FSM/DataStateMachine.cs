using System.Collections.Generic;
using UnityEngine;
using System;

public class DataStateMachine
{
    private readonly Dictionary<State, BaseState> _states;
    private readonly List<StateTransition> _transitions;
    private BaseState _currentState;
    private State _currentKey;

    public DataStateMachine(State initialKey,
                            Dictionary<State, BaseState> states,
                            List<StateTransition> transitions)
    {
        _states = states;
        _transitions = transitions;
        _currentKey = (State)(-1);
        ChangeState(initialKey);
    }

    public void UpdateState()
    {
        foreach (var t in _transitions)
        {
            if ((t.From == _currentKey || t.From == State.ANY)
                && t.Condition())
            {
                ChangeState(t.To);
                break;
            }
        }
        _currentState?.OperateUpdate();
    }

    public void FixedUpdateState() => _currentState?.OperateFixedUpdate();

    public void ChangeState(State newKey)
    {
        if (_currentKey == newKey) return;
        _currentState?.OperateExit();

        if (!_states.TryGetValue(newKey, out var next)) return;
        _currentKey = newKey;
        _currentState = next;
        _currentState.OperateEnter();
    }
}