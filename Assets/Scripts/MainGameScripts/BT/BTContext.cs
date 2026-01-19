using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BTContext
{
    public GameObject Owner { get; private set; }
    public Blackboard Blackboard { get; private set; }
    public float DeltaTime { get; internal set; }
    public float Time { get; internal set; }           // Time.time
    public float UnscaledTime { get; internal set; }   // Time.unscaledTime

    private List<TimerEntry> _timers = new();

    private struct TimerEntry
    {
        public float remaining;
        public Action callback;

        public TimerEntry(float time, Action cb)
        {
            remaining = time;
            callback = cb;
        }
    }

    public BTContext(GameObject owner, Blackboard bb)
    {
        Owner = owner;
        Blackboard = bb;
    }

    public void SetTimeout(float delay, Action callback)
    {
        Debug.Log($"[SetTimeout] 등록됨: {delay}초 후");

        if (delay <= 0f)
        {
            callback?.Invoke();
            return;
        }

        _timers.Add(new TimerEntry(delay, callback));
    }


    public void UpdateTimers()
    {
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            var t = _timers[i];
            t.remaining -= DeltaTime;

            if (t.remaining <= 0f)
            {
                Debug.Log("[SetTimeout] 실행됨");
                _timers[i] = _timers[_timers.Count - 1];
                _timers.RemoveAt(_timers.Count - 1);
                t.callback?.Invoke();
            }
            else
            {
                _timers[i] = t;
            }
        }
    }

    public void SetFrameTimes(float time, float unscaledTime, float deltaTime)
    {
        Time = time;
        UnscaledTime = unscaledTime;
        DeltaTime = deltaTime;
    }

}
