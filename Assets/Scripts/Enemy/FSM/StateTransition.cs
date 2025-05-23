using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTransition<T>
{
    public State From;
    public State To;
    public Func<T, bool> Condition;

    public StateTransition(State from, State to, Func<T, bool> condition)
    {
        From = from;
        To = to;
        Condition = condition;
    }
}