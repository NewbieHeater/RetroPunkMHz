// EnemyPatrol.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RigidNavigation))]
[DisallowMultipleComponent]
public sealed class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float moveSpeed = 0.8f;
    [SerializeField] private bool pingPong = true;
    [SerializeField] private float arriveEps = 0.05f;
    [SerializeField] private float minStartDist = 0.08f;
    [SerializeField] private float minProgressAfterAdvance = 0.08f;

    [Header("Surface Reaction")]
    [Tooltip("앞에 표면(벽/낭떠러지 코너)이 잡히면 그 표면에 붙음. False면 뒤돌아감")]
    [SerializeField] private bool attachSurfaceWhenAhead = true;

    [Tooltip("표면 반응 반복 방지(초)")]
    [SerializeField] private float surfaceReactCooldown = 0.15f;

    private IReadOnlyList<Vector3> pts = Array.Empty<Vector3>();
    private PatrolPoint[] defs = Array.Empty<PatrolPoint>();

    private int idx;
    public bool forward = true;

    private bool _waiting;
    private float _tWait;

    private Vector3 _lastAdvancePos;
    private bool _progressGateArmed;

    private RigidNavigation _nav;
    private PathProvider _pathProvider;

    private bool _returning;
    private int _returnIdx;

    private float _tLastSurfaceReact = -999f;

    private void Awake()
    {
        _nav = GetComponent<RigidNavigation>();
        InitPatrolPoints();
    }

    public void InitPatrolPoints()
    {
        if (!_pathProvider)
        {
            _pathProvider = GetComponent<PathProvider>();
            if (!_pathProvider)
            {
                pts = Array.Empty<Vector3>();
                defs = Array.Empty<PatrolPoint>();
                return;
            }
        }

        defs = _pathProvider.Definitions;
        pts = _pathProvider.BuildWorldPoints(transform);

        idx = 0;
        forward = true;
        _waiting = false;
        _returning = false;
        _progressGateArmed = false;
    }

    public void BeginPatrol()
    {
        if (pts == null || pts.Count == 0 || !_nav) return;
        if (_returning) return;

        int startIdx = idx;

        if (pts.Count >= 2 &&
            Mathf.Abs(pts[0].x - transform.position.x) < arriveEps &&
            Mathf.Abs(pts[0].y - transform.position.y) < arriveEps)
        {
            startIdx = 1;
        }

        idx = FindNextUsableIndex(startIdx);

        _nav.isStopped = false;
        _nav.SetSpeed(moveSpeed);
        _nav.SetDestination(pts[idx]);

        _lastAdvancePos = transform.position;
        _progressGateArmed = true;
    }

    public void Tick()
    {
        if (pts == null || pts.Count == 0 || !_nav) return;

        // Nav가 정렬 단계면 Patrol은 관여하지 않음
        if (_nav.IsAligningToSurface) return;

        // 1) "앞의 표면" 반응
        if (!_returning && !_waiting && _nav.hasPath && !_nav.isStopped)
        {
            if (Time.time - _tLastSurfaceReact >= surfaceReactCooldown)
            {
                if (attachSurfaceWhenAhead)
                {
                    if (!_nav.AlignLatched && _nav.TryGetAheadSurfaceNormal(out Vector3 n))
                    {
                        _tLastSurfaceReact = Time.time;
                        _nav.StartSurfaceAlign(n);
                        return;
                    }
                }
            }
        }

        // 2) 복귀
        if (_returning)
        {
            if (HasArrived())
            {
                _nav.ResetPath();
                idx = _returnIdx;
                _returning = false;

                AdvanceOnce();

                // ===== 핵심 수정: ResetPath 이후 반드시 재이동 허용 =====
                _nav.isStopped = false;

                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx]);

                _lastAdvancePos = transform.position;
                _progressGateArmed = true;
            }
            return;
        }

        // 3) 대기
        if (_waiting)
        {
            if (Time.time - _tWait >= defs[idx].dwellTime)
            {
                _waiting = false;

                AdvanceOnce();

                // ===== 핵심 수정: 대기 종료 후 반드시 재이동 허용 =====
                _nav.isStopped = false;

                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx]);

                _lastAdvancePos = transform.position;
                _progressGateArmed = true;
            }
            return;
        }

        // 4) 도착 처리
        if (HasArrived())
        {
            _nav.ResetPath();

            if (defs[idx].dwellTime > 0f)
            {
                _waiting = true;
                _tWait = Time.time;
                return;
            }

            AdvanceOnce();

            // ===== 핵심 수정: 도착 직후 다음 목적지로 갈 때 반드시 재이동 허용 =====
            _nav.isStopped = false;

            _nav.SetSpeed(moveSpeed);
            _nav.SetDestination(pts[idx]);

            _lastAdvancePos = transform.position;
            _progressGateArmed = true;
            return;
        }
    }

    private bool HasArrived()
    {
        if (_progressGateArmed)
        {
            if (Vector3.Distance(transform.position, _lastAdvancePos) < minProgressAfterAdvance)
                return false;

            _progressGateArmed = false;
        }

        float rem = _nav.RemainingDistanceX();
        if (float.IsNaN(rem) || float.IsInfinity(rem))
        {
            int checkIdx = _returning ? _returnIdx : idx;
            rem = Vector3.Distance(transform.position, pts[checkIdx]);
        }

        return rem <= arriveEps;
    }

    private int FindNextUsableIndex(int startIdx)
    {
        if (pts == null || pts.Count == 0) return startIdx;
        int len = pts.Count;
        int i = startIdx;
        int guard = len;

        while (guard-- > 0)
        {
            float dx = Mathf.Abs(pts[i].x - transform.position.x);
            float dy = Mathf.Abs(pts[i].y - transform.position.y);
            if (dx < minStartDist && dy < minStartDist)
            {
                var nxt = GetNextIndexPingPong(i, forward, len);
                i = nxt.nextIdx;
                forward = nxt.nextForward;
                continue;
            }
            break;
        }
        return i;
    }

    private (int nextIdx, bool nextForward) GetNextIndexPingPong(int i, bool fwd, int len)
    {
        if (!pingPong) return ((i + 1) % len, fwd);

        if (fwd)
        {
            if (i + 1 < len) return (i + 1, true);
            Debug.Log("e");
            return (Mathf.Max(len - 2, 0), false);
        }
        else
        {
            if (i - 1 >= 0) return (i - 1, false);
            return (Mathf.Min(1, len - 1), true);
        }
    }

    private void AdvanceOnce()
    {
        int len = (pts != null) ? pts.Count : 0;
        if (len <= 1) return;

        var nxt = GetNextIndexPingPong(idx, forward, len);
        idx = nxt.nextIdx;
        forward = nxt.nextForward;
    }
}
