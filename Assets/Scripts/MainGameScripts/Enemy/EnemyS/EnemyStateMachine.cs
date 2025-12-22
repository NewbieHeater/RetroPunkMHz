using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyStateMachine
{
    private readonly Dictionary<StateInfo, IEnemyState> _states
        = new Dictionary<StateInfo, IEnemyState>();

    public IEnemyState Current { get; private set; }

    public void Register(IEnemyState state)
    {
        if (state == null)
        {
            Debug.LogWarning("EnemyStateMachine.Register: null state");
            return;
        }

        _states[state.Kind] = state;
    }

    public void Initialize(StateInfo initial)
    {
        if (!_states.TryGetValue(initial, out var s))
        {
            Debug.LogError($"EnemyStateMachine.Initialize: 초기 상태 {initial} 없음");
            return;
        }

        Current = s;
        Current.Enter();
    }

    public void Change(StateInfo next)
    {
        if (Current != null && Current.Kind == next)
            return;

        if (!_states.TryGetValue(next, out var s))
        {
            Debug.LogError($"EnemyStateMachine.Change: 상태 {next} 없음");
            return;
        }

        Current?.Exit();
        Current = s;
        Current.Enter();
    }

    public void Tick(float deltaTime)
    {
        Current?.Tick(deltaTime);
    }
}
