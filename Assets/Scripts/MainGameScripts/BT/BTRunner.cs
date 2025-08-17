using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTRunner : MonoBehaviour
{
    [Tooltip("0이면 매 프레임 Tick. >0이면 해당 간격(초)마다 Tick.")]
    public float tickInterval = 0f;

    protected Blackboard Blackboard { get; private set; }
    protected BTContext Ctx { get; private set; }
    protected BTNode Root { get; private set; }

    private float _accum;

    protected virtual void Awake()
    {
        Blackboard = new Blackboard();
        Ctx = new BTContext(gameObject, Blackboard);
        Root = BuildTree();
        if (Root == null) Debug.LogWarning($"{name}: BuildTree()가 null을 반환했습니다.");
    }

    protected virtual BTNode BuildTree() { return null; } // 상속해서 구현

    protected virtual void Update()
    {
        if (Root == null) return;

        _accum += Time.deltaTime;
        if (tickInterval == 0f || _accum >= tickInterval)
        {
            Ctx.DeltaTime = _accum > 0f ? _accum : Time.deltaTime;
            _accum = 0f;
            Root.Tick(Ctx);
        }
    }

    public void ResetTree()
    {
        Root?.Reset(Ctx);
    }
}
