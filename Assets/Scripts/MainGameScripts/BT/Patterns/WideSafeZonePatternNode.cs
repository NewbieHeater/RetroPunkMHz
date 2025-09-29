using UnityEngine;

/// <summary>
/// 보스 정면 앞에 안전지대 프리팹을 소환하고, 그 영역 바깥에 있는 플레이어에게
/// 주기적으로 데미지를 가하는 광역 패턴.
/// </summary>
public class WideSafeZonePatternNode : PatternNodeBase
{
    private readonly Transform player;
    private readonly GameObject safeZonePrefab;

    private readonly float radius;
    private readonly float forwardOffset;
    private readonly float preDelay;
    private readonly float duration;
    private readonly float tickInterval;
    private readonly float damagePerTick;
    private readonly bool alignToGround; // true면 지면에 붙여 소환, false면 owner의 높이 기준

    // 내부 상태
    private GameObject _zone;
    private GameObject _telegraph;  // 선택(원하면 텔레그래프 프리팹 넣어도 됨)
    private Vector3 _center;
    private float _preTimer = -1f;
    private float _durTimer = -1f;
    private float _tickTimer = -1f;

    /// <param name="owner">보스 Transform</param>
    /// <param name="player">플레이어 Transform</param>
    /// <param name="groundMask">지면 레이어(alignToGround=true일 때 사용)</param>
    /// <param name="safeZonePrefab">안전지대 프리팹(AOEShieldZone 컴포넌트 포함)</param>
    /// <param name="radius">안전지대 반경</param>
    /// <param name="forwardOffset">보스 정면으로부터의 오프셋</param>
    /// <param name="preDelay">발동 전 경고 시간</param>
    /// <param name="duration">유지 시간</param>
    /// <param name="tickInterval">바깥에 있을 때 데미지 주기</param>
    /// <param name="damagePerTick">틱 당 데미지</param>
    /// <param name="alignToGround">지면 정렬 여부(true면 지면 높이에 배치)</param>
    public WideSafeZonePatternNode(
        Transform owner, Transform player, LayerMask groundMask,
        GameObject safeZonePrefab,
        float radius = 3f,
        float forwardOffset = 0f,
        float preDelay = 0.8f,
        float duration = 10f,
        float tickInterval = 0.5f,
        float damagePerTick = 10f,
        bool alignToGround = true
    ) : base(owner, null, groundMask)
    {
        this.player = player;
        this.safeZonePrefab = safeZonePrefab;
        this.radius = Mathf.Max(0.1f, radius);
        this.forwardOffset = forwardOffset;
        this.preDelay = Mathf.Max(0f, preDelay);
        this.duration = 10f;
        this.tickInterval = Mathf.Max(0.05f, tickInterval);
        this.damagePerTick = Mathf.Max(0f, damagePerTick);
        this.alignToGround = alignToGround;
    }

    public override void NodeStart(BTContext ctx)
    {
        base.NodeStart(ctx);

        // 1) 보스 정면 기준 중심점 계산
        Vector3 basePos = owner ? owner.position + owner.forward * forwardOffset : Vector3.zero;

        if (alignToGround)
        {
            // 지면 스냅(XZ만 고려)
            _center = SnapToGround(new Vector3(basePos.x, 0f, basePos.z));
        }
        else
        {
            _center = basePos;
        }

        // (선택) 텔레그래프를 쓰고 싶다면 여기에 생성하면 됨.
        // _telegraph = Object.Instantiate(telegraphPrefab, _center, Quaternion.identity);

        _preTimer = preDelay;
        _durTimer = -1f;
        _tickTimer = -1f;
        _zone = null;
    }

    public override NodeStatus Tick(BTContext ctx)
    {
        //if (!running) return NodeStatus.Failure;
        // A) 프리 딜레이: 경고 시간
        if (_preTimer >= 0f)
        {
            _preTimer -= ctx.DeltaTime;
            if (_preTimer <= 0f)
            {
                if (_telegraph) Object.Destroy(_telegraph);

                // 2) 안전지대 소환
                _zone = Object.Instantiate(safeZonePrefab, _center, Quaternion.LookRotation(owner ? owner.forward : Vector3.forward));
                Debug.Log(_zone.transform.position);
                _durTimer = duration;
                _tickTimer = 0f; // 바로 첫 틱 체크
            }
            return NodeStatus.Running;
        }

        // B) 유지 시간 동안 판정 & 데미지
        if (_durTimer > 0f)
        {
            _durTimer -= ctx.DeltaTime;
            _tickTimer += ctx.DeltaTime;

            if (_tickTimer >= tickInterval)
            {
                _tickTimer = 0f;

                // 플레이어가 안전지대 내부인지(XZ 기준)
                if (player)
                {
                    Vector2 p = new Vector2(player.position.x, player.position.z);
                    Vector2 c = new Vector2(_center.x, _center.z);
                    bool inside = Vector2.Distance(p, c) <= radius;

                    if (!inside && damagePerTick > 0f)
                    {
                        ShowHp.Instance.MinusTMP(damagePerTick);
                    }
                }
            }
            return NodeStatus.Running;
        }

        // C) 종료 처리
        return NodeStatus.Success;
    }

    public override void NodeStop(BTContext ctx, NodeStatus result)
    {
        base.NodeStop(ctx, result);
        if (_telegraph) Object.Destroy(_telegraph);
        if (_zone) Object.Destroy(_zone.gameObject);
    }
}
