using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command
{
    // 레코딩 시점(초)
    public float time;
    // 이 커맨드가 달려 있는 대상
    [NonSerialized] public GameObject target;

    public abstract void Execute();
    protected void Undo() { }
}
