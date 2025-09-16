using UnityEngine;

/// <summary>
/// 플레이어의 Z(레인)과 Y(높이)를 따라 X방향으로 좌→우 또는 우→좌로 기둥을 수평 이동시키는 패턴.
/// CylinderUnder가 -transform.up으로 이동하므로, 원하는 수평 이동 방향 d에 대해
/// transform.up = -d 로 회전시켜 EnableFall()을 재사용한다.
/// </summary>
public class SideSweepPatternNode : PatternNodeBase
{
    public enum Direction { LeftToRight, RightToLeft }

    // 필수
    private readonly Transform player;
    private readonly CylinderUnder pillarPrefab;

    // 맵/경로
    private readonly float mapWidth;    // 전체 가로 폭
    private readonly float margin;      // 화면 밖 여유(스폰/엔드 오버런)
    private readonly Direction dir;

    // 타이밍/비주얼
    private readonly float preDelay;    // 경고 표시 후 발동까지 대기
    private readonly float postWait;    // 도달 후 성공 반환까지 대기
    private readonly float moveSpeed;   // 수평 이동 속도
    private readonly float yOffset;     // 플레이어 높이에서 살짝 띄울 오프셋(시각/충돌 안정)
    private readonly float lineWidth;
    private readonly Color lineColor;

    // 내부 상태
    private GameObject lineGO;
    private CylinderUnder pillar;
    private Vector3 startPos, endPos;
    private float preTimer = -1f;
    private float runTimer = -1f;
    private float travelTime = 0f; // 이동 후 파괴용
    private float postTimer = -1f;

    /// <param name="owner">보스(맵 중심 기준 x/z 참고)</param>
    /// <param name="player">플레이어</param>
    /// <param name="groundMask">지면 레이어 (본 버전에서는 사용하지 않음)</param>
    /// <param name="pillarPrefab">수평 스윕에 사용할 기둥 (CylinderUnder 재사용)</param>
    /// <param name="mapWidth">맵 전체 가로 폭</param>
    /// <param name="margin">좌우 스폰/도착 오버런 여유</param>
    /// <param name="dir">좌→우 또는 우→좌</param>
    /// <param name="moveSpeed">수평 이동 속도</param>
    /// <param name="preDelay">텔레그래프 표시 시간</param>
    /// <param name="postWait">도착 후 대기 시간</param>
    /// <param name="yOffset">플레이어 높이 기준 추가 오프셋</param>
    /// <param name="lineWidth">텔레그래프 라인 두께</param>
    /// <param name="lineColorOpt">라인 색상 (null이면 빨강 75% 투명)</param>
    public SideSweepPatternNode(
        Transform owner, Transform player, LayerMask groundMask,
        CylinderUnder pillarPrefab,
        float mapWidth = 20f, float margin = 1.0f,
        Direction dir = Direction.LeftToRight,
        float moveSpeed = 35f,
        float preDelay = 0.7f, float postWait = 0.4f,
        float yOffset = 0.8f,
        float lineWidth = 0.05f,
        Color? lineColorOpt = null
    ) : base(owner, /*target*/ null, groundMask)
    {
        this.player = player;
        this.pillarPrefab = pillarPrefab;
        this.mapWidth = Mathf.Max(1f, mapWidth);
        this.margin = Mathf.Max(0f, margin);
        this.dir = dir;
        this.moveSpeed = Mathf.Max(0.1f, moveSpeed);
        this.preDelay = Mathf.Max(0f, preDelay);
        this.postWait = Mathf.Max(0f, postWait);
        this.yOffset = yOffset;
        this.lineWidth = lineWidth;
        this.lineColor = lineColorOpt ?? new Color(1f, 0f, 0f, 0.75f);
    }

    public override void NodeStart(BTContext ctx)
    {
        base.NodeStart(ctx);

        // 1) 레인(Z)와 높이(Y): 플레이어가 있으면 그 좌표, 없으면 보스 기준
        float laneZ = player ? player.position.z : (owner ? owner.position.z : 0f);
        float laneY = player ? player.position.y : (owner ? owner.position.y : 0f);

        // 2) 시작/끝 X 계산 (맵 중심 = owner.x 기준)
        float centerX = owner ? owner.position.x : 0f;
        float half = mapWidth * 0.5f;
        float leftX = centerX - half - margin;
        float rightX = centerX + half + margin;

        // 3) 시작/끝 위치(플레이어 Y 높이 기준)
        if (dir == Direction.LeftToRight)
        {
            startPos = new Vector3(leftX, laneY + yOffset, laneZ);
            endPos = new Vector3(rightX, laneY + yOffset, laneZ);
        }
        else // RightToLeft
        {
            startPos = new Vector3(rightX, laneY + yOffset, laneZ);
            endPos = new Vector3(leftX, laneY + yOffset, laneZ);
        }

        // 4) 텔레그래프: 수평 경로 라인(같은 Y 높이에서 표시)
        lineGO = new GameObject("TelegraphLine_SideSweep");
        var line = lineGO.AddComponent<TelegraphRing>(); // start/end 라인 버전
        line.Init(
            start: startPos + Vector3.up * 0f, // 이미 공중에 있으므로 추가 오프셋 불필요
            end: endPos + Vector3.up * 0f,
            duration: preDelay,
            width: lineWidth,
            color: lineColor
        );

        // 타이머 세팅
        preTimer = preDelay;
        runTimer = -1f;
        postTimer = -1f;
        pillar = null;

        // 이동 시간(파괴 시점) 계산
        float pathLen = Vector3.Distance(startPos, endPos);
        travelTime = (pathLen / moveSpeed) + 0.1f; // 약간의 버퍼
    }

    public override NodeStatus Tick(BTContext ctx)
    {
        //if (!running) return NodeStatus.Failure;
        // A) 경고 대기
        if (preTimer >= 0f)
        {
            preTimer -= ctx.DeltaTime;
            if (preTimer <= 0f)
            {
                if (lineGO) Object.Destroy(lineGO);

                // 기둥 스폰 + 수평 이동 시작
                pillar = Object.Instantiate(pillarPrefab, startPos, Quaternion.identity);

                // 수평 이동 방향 d에 대해 -up = d 가 되도록 회전
                Vector3 d = (endPos - startPos).normalized;
                if (d.sqrMagnitude < 1e-6f) d = Vector3.right;

                Quaternion rot = Quaternion.FromToRotation(Vector3.up, -d);
                pillar.transform.rotation = rot;

                // 기존 하강 로직 재사용(속도를 수평 이동 속도로 전달)
                pillar.Init(moveSpeed);
                pillar.EnableFall(0f);

                runTimer = travelTime;
                postTimer = -1f;
            }
            return NodeStatus.Running;
        }

        // B) 수평 주행 중
        if (pillar && runTimer > 0f)
        {
            runTimer -= ctx.DeltaTime;
            if (runTimer <= 0f)
            {
                // 지면 충돌 이벤트가 없으므로 수동 파괴
                Object.Destroy(pillar.gameObject);
                pillar = null;
                postTimer = postWait;
            }
            return NodeStatus.Running;
        }

        // C) 후행 연출 대기 → 성공
        if (postTimer > 0f)
        {
            postTimer -= ctx.DeltaTime;
            if (postTimer <= 0f) return NodeStatus.Success;
            return NodeStatus.Running;
        }

        return NodeStatus.Success;
    }

    public override void NodeStop(BTContext ctx, NodeStatus result)
    {
        base.NodeStop(ctx, result);
        if (lineGO) Object.Destroy(lineGO);
        if (pillar) Object.Destroy(pillar.gameObject);
    }
}
