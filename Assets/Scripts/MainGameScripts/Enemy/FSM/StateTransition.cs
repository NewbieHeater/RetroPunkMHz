using System;

public class StateTransition<TContext>
    where TContext : EnemyFSMBase<TContext>
{
    public State From;
    public State To;
    private readonly Func<bool> _condition;
    public StateTransition(State from, State to, Func<bool> condition)
    {
        From = from;
        To = to;
        _condition = condition;
    }
    public bool Condition() => _condition();
}