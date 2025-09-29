using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boss : BTRunner
{
    [Header("Target/Perception")]
    [SerializeField] private Transform player;   // 플레이어
    [SerializeField] private float sightRange = 12f;

    [Header("Pattern Prefabs/Settings")]
    [SerializeField] private CylinderUnder pillarPrefab;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float mapWidth = 20f;
    [SerializeField] private float spacing = 4f;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float spawnHeight = 10f;
    // 패턴 노드 런타임 인스턴스
    private DropMultiplePatternNode _dropNodeMulti;
    private DropPatternNode _dropNode;
    private SideSweepPatternNode _sideL2R;
    private SideSweepPatternNode _sideR2L;
    private WideSafeZonePatternNode _wideSafe;
    [Header("Wide SafeZone")]
    [SerializeField] private GameObject safeZonePrefab;
    [SerializeField] private float wideRadius = 1.5f;
    [SerializeField] private float wideDuration = 15f;
    [SerializeField] private float widePreDelay = 0.8f;
    [SerializeField] private float wideForwardOffset = 0f;
    [SerializeField] private float wideTickInterval = 0.5f;
    [SerializeField] private float wideDamagePerTick = 10f;

    protected override void Awake()
    {
        // DropMultiplePatternNode 생성
        _dropNodeMulti = new DropMultiplePatternNode(
            owner: transform,
            groundMask: groundMask,
            pillarPrefab: pillarPrefab,
            mapWidth: mapWidth,
            spacing: spacing,
            interval: interval,
            spawnHeight: spawnHeight
        );

        _dropNode = new DropPatternNode(
            owner: transform,
            player: player,
            groundMask: groundMask,
            pillarPrefab: pillarPrefab,
            mapWidth: mapWidth, spacing: spacing, interval: interval, spawnHeight: spawnHeight,
            preDelay: 0.9f, postWait: 1.5f, fallSpeed: 22f,
            ringRadius: 0.6f, ringWidth: 1.5f, faceSlope: true,
            ringColorOpt: new Color(1f, 0.4f, 0f, 0.8f) // 주황빛
        );

        base.Awake();
       
    }
    bool InConcentrationMode = false;
    private bool inCombo = true;

    protected override BTNode BuildTree()
    {
        bool HasTarget(BTContext ctx) =>
            player && Vector3.Distance(transform.position, player.position) <= sightRange;

        return new BTBuilder()
    .Selector("Root")
        .Guard(HasTarget, b =>
        {
            b.Sequence("AttackCombo")
                .Do("MoveToPointTick", MoveToPointTick)

// Wide SafeZone
.Do("Wide SafeZone",
    ctx => _wideSafe.Tick(ctx),
    onStart: ctx =>
    {
        _wideSafe = new WideSafeZonePatternNode(
            owner: transform, player: player, groundMask: groundMask,
            safeZonePrefab: safeZonePrefab,
            radius: 2f,
            forwardOffset: 0f,
            preDelay: widePreDelay,
            duration: wideDuration,           // 필드 사용
            tickInterval: wideTickInterval,
            damagePerTick: wideDamagePerTick,
            alignToGround: true
        );
        _wideSafe.NodeStart(ctx);
    },
    onStop: (ctx, res) =>
    {
        _wideSafe?.NodeStop(ctx, res);
        _wideSafe = null; // 다음 실행 때 새로 시작하도록
    }
)
.Do("DropSolo",
                    ctx => _dropNode.Tick(ctx),
                    onStart: ctx =>
                    {
                        _dropNode = new DropPatternNode(
                            owner: transform,
                            player: player,
                            groundMask: groundMask,
                            pillarPrefab: pillarPrefab,
                            mapWidth: mapWidth, spacing: spacing, interval: interval,
                            spawnHeight: spawnHeight,
                            preDelay: 0.9f,
                            postWait: 1.5f,
                            fallSpeed: 22f,
                            ringRadius: 0.6f,
                            ringWidth: 1.5f,
                            faceSlope: true,
                            ringColorOpt: new Color(1f, 0.4f, 0f, 0.8f)
                        );
                        _dropNode.NodeStart(ctx);
                    },
                    onStop: (ctx, res) =>
                    {
                        _dropNode?.NodeStop(ctx, res);
                        _dropNode = null;
                    }
                )
.Wait(1.0f)
// SideSweep L→R
.Do("SideSweep L→R",
    ctx => _sideL2R.Tick(ctx),
    onStart: ctx =>
    {
        _sideL2R = new SideSweepPatternNode(
            owner: transform, player: player, groundMask: groundMask,
            pillarPrefab: pillarPrefab, mapWidth: mapWidth,
            dir: SideSweepPatternNode.Direction.LeftToRight
        );
        _sideL2R.NodeStart(ctx);
    },
    onStop: (ctx, res) =>
    {
        _sideL2R?.NodeStop(ctx, res);
        _sideL2R = null;
    }
)

// SideSweep R→L
.Do("SideSweep R→L",
    ctx => _sideR2L.Tick(ctx),
    onStart: ctx =>
    {
        _sideR2L = new SideSweepPatternNode(
            owner: transform, player: player, groundMask: groundMask,
            pillarPrefab: pillarPrefab, mapWidth: mapWidth,
            dir: SideSweepPatternNode.Direction.RightToLeft
        );
        _sideR2L.NodeStart(ctx);
    },
    onStop: (ctx, res) =>
    {
        _sideR2L?.NodeStop(ctx, res);
        _sideR2L = null;
    }
)
.Wait(3.0f)
.Do("DropMultiple",
                    ctx => _dropNodeMulti.Tick(ctx),
                    onStart: ctx =>
                    {
                        _dropNodeMulti = new DropMultiplePatternNode(
                            owner: transform,
                            groundMask: groundMask,
                            pillarPrefab: pillarPrefab,
                            mapWidth: mapWidth,
                            spacing: spacing,
                            interval: interval,
                            spawnHeight: spawnHeight
                        );
                        _dropNodeMulti.NodeStart(ctx);
                    },
                    onStop: (ctx, res) =>
                    {
                        _dropNodeMulti?.NodeStop(ctx, res);
                        _dropNodeMulti = null;
                    }
                )

            .End();
        })
    .End()
    .Build();

    }
    public CinemachineCameraController cameraController;
    private NodeStatus MoveToPointTick(BTContext ctx)
    {
        if (!InConcentrationMode)
        {
            InConcentrationMode = true;
            cameraController.IsManualMode = true;
        }
        return NodeStatus.Success;
    }


}
