using System.Collections.Generic;

public class DataStateMachine<TSelf>
    where TSelf : EnemyFSMBase<TSelf>
{
    private readonly Dictionary<State, BaseState<TSelf>> _states;
    private readonly List<StateTransition<TSelf>> _transitions;
    private BaseState<TSelf> _currentState;
    private State _currentKey;

    public DataStateMachine(State initialKey,
                            Dictionary<State, BaseState<TSelf>> states,
                            List<StateTransition<TSelf>> transitions)
    {
        _states = states;
        _transitions = transitions;
        _currentKey = (State)(-1);
        ChangeState(initialKey);
    }

    public void UpdateState()
    {
        foreach (var t in _transitions)
            if ((t.From == _currentKey || t.From == State.ANY) && t.Condition())
                ChangeState(t.To);
        _currentState.OperateUpdate();
    }

    public void FixedUpdateState() => _currentState.OperateFixedUpdate();

    public void ChangeState(State newKey)
    {
        if (_currentState != null && _currentKey == newKey)
            return;
        _currentState?.OperateExit();
        if (!_states.TryGetValue(newKey, out var next) || next == null)
        {
            return;
        }
        _currentKey = newKey;
        _currentState = next;
        _currentState.OperateEnter();
    }
}