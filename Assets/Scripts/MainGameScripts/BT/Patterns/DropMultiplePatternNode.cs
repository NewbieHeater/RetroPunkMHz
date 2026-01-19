using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 중심(owner.position.x)을 기준으로 좌(-width/2) → 우(+width/2) 방향으로
/// spacing 간격마다 기둥을 0.5초 간격으로 떨어뜨리는 패턴 노드.
/// </summary>
public sealed class DropMultiplePatternNode : PatternNodeBase
{
    // 프리팹 & 배치 파라미터
    [SerializeField] private CylinderUnder pillarPrefab; // <- GameObject 대신
    private readonly float mapWidth;      // 예: 20
    private readonly float spacing;       // 예: 4
    private readonly float interval;      // 예: 0.5
    private readonly float spawnHeight;   // 예: 10 (지면에서 얼마 위에서 소환할지)
    private List<CylinderUnder> spawnedCylinders = new();  // 추가
    private bool fallTriggered = false;
    // 진행 상태
    private int totalSlots;   // 생성될 총 위치 개수
    private int index;        // 현재 생성할 슬롯 인덱스(0부터)
    private float leftEdgeX;  // 맵 왼쪽 X
    private float baseZ;      // 고정 Z (owner 기준)

    /// <param name="owner">보스 트랜스폼 (맵 중심으로 사용)</param>
    /// <param name="groundMask">지면 스냅용 레이어</param>
    /// <param name="pillarPrefab">기둥 프리팹(움직이지 않는 고정형/낙하형 아무거나)</param>
    /// <param name="mapWidth">맵 가로 길이 (예: 20)</param>
    /// <param name="spacing">기둥 간 간격 (예: 4)</param>
    /// <param name="interval">소환 간격 초 (예: 0.5)</param>
    /// <param name="spawnHeight">지면에서 얼마 위에서 생성할지 (예: 10)</param>
    public DropMultiplePatternNode(
        Transform owner, LayerMask groundMask,
        CylinderUnder pillarPrefab,
        float mapWidth = 20f, float spacing = 4f,
        float interval = 0.5f, float spawnHeight = 10f
    ) : base(owner, /*target*/ null, groundMask)
    {
        this.pillarPrefab = pillarPrefab;
        this.mapWidth = Mathf.Max(0.01f, mapWidth);
        this.spacing = Mathf.Max(0.01f, spacing);
        this.interval = Mathf.Max(0f, interval);
        this.spawnHeight = spawnHeight;
    }

    public override void NodeStart(BTContext ctx)
    {
        base.NodeStart(ctx);
        spawnedCylinders.Clear();
        fallTriggered = false;
        if (pillarPrefab == null)
        {
            Debug.LogWarning("[DropMultiplePatternNode] pillarPrefab 미할당");
            running = false;
            return;
        }

        // 맵 좌우 에지 & 슬롯 수 계산
        float centerX = owner ? owner.position.x : 0f;
        leftEdgeX = centerX - mapWidth * 0.5f;
        baseZ = owner ? owner.position.z : 0f;

        // 예: width=20, spacing=4  →  floor(20/4)=5, +1 = 6개  → X: -10,-6,-2,2,6,10
        totalSlots = Mathf.FloorToInt(mapWidth / spacing) + 1;
        index = 0;

        // 시작 즉시 1개 생성되도록 타이머 0
        timer = 0f;
    }
    private float postFallWait = 1f; // 기둥 낙하 후 보여줄 시간
    private float postFallTimer = -1f;
    private float fallSpeed = 35;
    public override NodeStatus Tick(BTContext ctx)
    {
        if (!running) return NodeStatus.Failure;
        if (index < totalSlots)
        {
            timer -= ctx.DeltaTime;
            if (timer <= 0f)
            {
                float x = leftEdgeX + spacing * index;
                Vector3 xz = new Vector3(x, 0f, baseZ);
                Vector3 land = SnapToGround(xz);
                Vector3 spawn = land + Vector3.up * spawnHeight;

                CylinderUnder cylinder = Object.Instantiate(pillarPrefab, spawn, Quaternion.identity);
                cylinder.Init(fallSpeed);        // 패턴마다 속도 주입
                if (cylinder != null)
                    spawnedCylinders.Add(cylinder);

                index++;
                timer = interval;
            }

            return NodeStatus.Running;
        }

        // 모든 기둥 소환 완료 후 1초 뒤 낙하
        if (!fallTriggered)
        {
            fallTriggered = true;
            ctx.SetTimeout(1f, () =>
            {
                foreach (var cylinder in spawnedCylinders)
                    cylinder?.EnableFall(0f);

                postFallTimer = postFallWait; // 낙하 후 보여줄 시간
            });
        }

        // 낙하 후 약간의 시간 대기
        if (fallTriggered && postFallTimer > 0f)
        {
            postFallTimer -= ctx.DeltaTime;
            if (postFallTimer <= 0f)
            {
                return NodeStatus.Success;
            }
        }

        return NodeStatus.Running;
    }


    public override void NodeStop(BTContext ctx, NodeStatus result)
    {
        base.NodeStop(ctx, result);
        // 필요시 남은 스폰 취소 로직 추가 가능(현재는 소환 즉시 끝이라 정리 불필요)
    }
}
