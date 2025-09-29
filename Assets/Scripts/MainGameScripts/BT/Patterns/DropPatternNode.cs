using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 x 위치에 경고 링(TelegraphRing)을 잠깐 띄운 뒤,
/// 지정 높이에서 기둥을 소환하여 즉시 낙하시키는 단일 드롭 패턴.
/// </summary>
public class DropPatternNode : PatternNodeBase
{
    // 필수 레퍼런스
    private readonly Transform player;
    private readonly CylinderUnder pillarPrefab;

    // 드롭 파라미터
    private readonly float spawnHeight;
    private readonly float fallSpeed;

    // 텔레그래프 파라미터
    private readonly float preDelay;     // 드롭 전 경고 표시 시간
    private readonly float postWait;     // 드롭 후 연출을 보여줄 시간
    private readonly float ringRadius;
    private readonly float ringWidth;
    private readonly Color ringColor;
    private readonly bool faceSlope;    // 경사면 노멀 정렬 여부

    // 내부 상태
    private Vector3 cachedLand;
    private Vector3 cachedNormal = Vector3.up;
    private float baseZ;
    private float preTimer = -1f;
    private float postTimer = -1f;
    private bool pillarSpawned;
    private GameObject lineGO;

    /// <param name="owner">보스 트랜스폼 (맵 중심 z 기준)</param>
    /// <param name="player">플레이어 트랜스폼</param>
    /// <param name="groundMask">지면 레이어</param>
    /// <param name="pillarPrefab">낙하 기둥 프리팹(CylinderUnder 컴포넌트)</param>
    /// <param name="mapWidth">호환성 유지용(미사용)</param>
    /// <param name="spacing">호환성 유지용(미사용)</param>
    /// <param name="interval">호환성 유지용(미사용)</param>
    /// <param name="spawnHeight">지면 위 스폰 높이</param>
    /// <param name="preDelay">드롭 전 경고 표시 시간</param>
    /// <param name="postWait">드롭 후 대기(성공 반환까지)</param>
    /// <param name="fallSpeed">기둥 낙하 속도</param>
    /// <param name="ringRadius">경고 링 반경</param>
    /// <param name="ringWidth">경고 링 두께</param>
    /// <param name="faceSlope">경사면 노멀 정렬 여부</param>
    /// <param name="ringColorOpt">경고 색 (null이면 빨강 75% 투명)</param>
    public DropPatternNode(
        Transform owner, Transform player, LayerMask groundMask,
        CylinderUnder pillarPrefab,
        float mapWidth = 20f, float spacing = 4f,
        float interval = 0.5f, float spawnHeight = 10f,
        float preDelay = 0.8f, float postWait = 2f, float fallSpeed = 20f,
        float ringRadius = 1.5f, float ringWidth = 0.05f, bool faceSlope = true,
        Color? ringColorOpt = null
    ) : base(owner, /*target*/ null, groundMask)
    {
        this.player = player;
        this.pillarPrefab = pillarPrefab;
        this.spawnHeight = spawnHeight;
        this.preDelay = Mathf.Max(0f, preDelay);
        this.postWait = Mathf.Max(0f, postWait);
        this.fallSpeed = fallSpeed;
        this.ringRadius = ringRadius;
        this.ringWidth = ringWidth;
        this.faceSlope = faceSlope;
        this.ringColor = ringColorOpt ?? new Color(1f, 0f, 0f, 0.75f);
    }

    public override void NodeStart(BTContext ctx)
    {
        base.NodeStart(ctx);

        pillarSpawned = false;

        float x = player ? player.position.x : 0f;
        baseZ = owner ? owner.position.z : 0f;

        Vector3 xz = new Vector3(x, 0f, baseZ);
        cachedLand = SnapToGround(xz);

        if (faceSlope)
        {
            var rayOrigin = cachedLand + Vector3.up * (spawnHeight + 0.5f);
            if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, spawnHeight + 2f, groundMask))
                cachedNormal = hit.normal;
            else
                cachedNormal = Vector3.up;
        }
        else cachedNormal = Vector3.up;

        // ▼▼▼ 여기부터 변경: "수직 경고 라인" 생성 ▼▼▼
        Vector3 spawn = cachedLand + Vector3.up * spawnHeight;

        lineGO = new GameObject("TelegraphRing");
        var line = lineGO.AddComponent<TelegraphRing>();
        // z-fighting 피하려고 지면에서 살짝 띄움 (노멀 방향 2cm)
        var landViz = cachedLand + cachedNormal * 0.02f;
        line.Init(start: spawn + Vector3.up, end: landViz, duration: preDelay, width: ringWidth, color: ringColor);
        // ▲▲▲ 변경 끝 ▲▲▲

        preTimer = preDelay;
        postTimer = -1f;
    }


    public override NodeStatus Tick(BTContext ctx)
    {
        if (!pillarSpawned && preTimer >= 0f)
        {
            preTimer -= ctx.DeltaTime;
            if (preTimer <= 0f)
            {
                if (lineGO) Object.Destroy(lineGO);   // 기존 ringGO → lineGO

                // 기둥 스폰 + 즉시 낙하
                Vector3 spawn = cachedLand + Vector3.up * spawnHeight;
                var cylinder = Object.Instantiate(pillarPrefab, spawn, Quaternion.identity);
                cylinder.Init(fallSpeed);
                cylinder.EnableFall(0f);

                pillarSpawned = true;
                postTimer = postWait;
            }
            return NodeStatus.Running;
        }


        // 3) 낙하 후 잠깐 대기 → 완료
        if (pillarSpawned && postTimer > 0f)
        {
            postTimer -= ctx.DeltaTime;
            if (postTimer <= 0f)
                return NodeStatus.Success;

            return NodeStatus.Running;
        }

        return NodeStatus.Success;
    }

    public override void NodeStop(BTContext ctx, NodeStatus result)
    {
        base.NodeStop(ctx, result);
        if (lineGO) Object.Destroy(lineGO); // 기존 ringGO → lineGO
    }

}
