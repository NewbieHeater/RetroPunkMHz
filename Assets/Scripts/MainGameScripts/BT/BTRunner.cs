using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTRunner : MonoBehaviour
{
    [Tooltip("0이면 매 프레임 Tick. >0이면 해당 간격(초)마다 Tick.")]
    public float tickInterval = 0f;

    [Tooltip("Tick 간격 계산에 Time.timeScale의 영향을 받지 않게 하려면 체크")]
    public bool useUnscaledForTick = false;

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

        Ctx.Time = UnityEngine.Time.time;
        Ctx.UnscaledTime = UnityEngine.Time.unscaledTime;

        float dtSource = useUnscaledForTick
            ? UnityEngine.Time.unscaledDeltaTime
            : UnityEngine.Time.deltaTime;
        _accum += dtSource;

        Ctx.UpdateTimers();

        // (4) Tick 실행
        if (tickInterval == 0f || _accum >= tickInterval)
        {
            Ctx.DeltaTime = (tickInterval > 0f) ? _accum : dtSource;
            _accum = 0f;
            Root.Tick(Ctx);
        }
    }

    public void ResetTree()
    {
        Root?.Reset(Ctx);
    }
}
