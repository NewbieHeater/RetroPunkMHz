using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySight : MonoBehaviour
{
    [Header("시야 설정 (2D 플랫포머 기준)")]
    [SerializeField] private float _horizontalRange = 6f;     // 좌우 감지 거리
    [SerializeField] private float _viewAngle = 180f;  // 위/아래 허용 높이 (절반)
    [SerializeField] private bool _respectFacing = true;      // 적이 바라보는 방향 앞쪽만 볼지 여부

    [Header("레이캐스트/레이어 설정")]
    [SerializeField] private LayerMask _obstacleMask;         // 벽/지형 레이어
    [SerializeField] private string _playerTag = "Player";    // 플레이어 태그
    [SerializeField] private string _playerHitboxLayerName = "PlayerHitBox";
    [SerializeField] private Transform _eye;                  // 눈 위치 (없으면 transform)

    private Transform Eye => _eye ? _eye : transform;

    /// <summary>
    /// 2D 플랫포머용 시야 체크
    /// - X축: horizontalRange 이내
    /// - Y축: ±verticalHalfHeight 이내
    /// - (옵션) 적이 바라보는 방향 앞쪽만 허용
    /// - Raycast로 PlayerHitBox까지 가리는 장애물이 없는지 확인
    /// </summary>
    public bool IsTargetInSight(Transform target, float rangeOverride = -1f)
    {
        if (!target) return false;

        float range = (rangeOverride > 0f) ? rangeOverride : _horizontalRange;

        Vector3 eyePos = Eye.position;
        Vector3 targetPos = target.position;

        // 1) 평면화 (2D 판단)
        Vector3 dir = targetPos - eyePos;
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist > range)
            return false;

        // 2) forward 역시 평면화 (회전 기반 방향)
        Vector3 forward = transform.forward;
        forward.y = 0f;

        // 3) 각도 계산
        float angle = Vector3.Angle(forward, dir);
        if (angle > _viewAngle * 0.5f)
            return false;
        // 4) 시야에 들어오면 Raycast로 막혔는지 확인
        return HasLineOfSight3D(eyePos, targetPos);
    }




    private bool HasLineOfSight3D(Vector3 origin, Vector3 targetPos)
    {
        Vector3 dir = targetPos - origin;
        float dist = dir.magnitude;
        if (dist <= 1e-4f)
            return true;

        dir /= dist;

        int losMask = _obstacleMask;
        int playerLayer = LayerMask.NameToLayer(_playerHitboxLayerName);
        if (playerLayer >= 0)
            losMask |= (1 << playerLayer);

        if (Physics.Raycast(origin, dir, out var hit, dist, losMask, QueryTriggerInteraction.Ignore))
        {
            // "PlayerHitBox" 레이어에 있는 콜라이더이면서, 루트 또는 부모에 Player 태그가 있는지 검사
            if (hit.collider.CompareTag(_playerTag))
                return true;

            // 히트한 콜라이더의 부모에 Player 태그가 있을 수도 있음
            var root = hit.collider.transform.root;
            if (root != null && root.CompareTag(_playerTag))
                return true;

            // 그 외는 장애물로 간주
            return false;
        }

        // 아무 것도 안 맞으면 가려진 게 없다고 보고 true
        return true;
    }


}
