using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command
{
    // ���ڵ� ����(��)
    public float time;
    // �� Ŀ�ǵ尡 �޷� �ִ� ���
    [NonSerialized] public GameObject target;

    public abstract void Execute();
    protected void Undo() { }
}
