using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RigidNavigation))]
[DisallowMultipleComponent]
public sealed class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private PathProvider _pathProvider;
    [SerializeField] private float moveSpeed = 0.8f;
    [SerializeField] private float rotateDegPerSec = 180f;
    [SerializeField] private bool pingPong = true;
    [SerializeField] private float arriveEps = 0.05f;
    [SerializeField] private float minStartDist = 0.08f;
    [SerializeField] private float minProgressAfterAdvance = 0.08f;

    private IReadOnlyList<Vector3> pts = Array.Empty<Vector3>();
    private PatrolPoint[] defs = Array.Empty<PatrolPoint>();

    private int idx;
    private bool forward = true;
    private bool _waiting;
    private float _tWait;
    private Vector3 _lastAdvancePos;
    private bool _progressGateArmed;

    private RigidNavigation _nav;

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
    }

    public void BeginPatrol(RigidNavigation.MoveMode mode)
    {
        if (pts == null || pts.Count == 0 || !_nav) return;

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
        _nav.SetDestination(pts[idx], mode);

        _lastAdvancePos = transform.position;
        _progressGateArmed = true;
    }

    public void Tick(RigidNavigation.MoveMode mode)
    {
        if (pts == null || pts.Count == 0 || !_nav) return;

        if (_waiting)
        {
            if (Time.time - _tWait >= defs[idx].dwellTime)
            {
                _waiting = false;
                AdvanceOnce();
                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx], mode);
                _lastAdvancePos = transform.position;
                _progressGateArmed = true;
            }
            else
            {
                FaceTowardsX(pts[idx].x);
            }
            return;
        }

        if (HasArrived())
        {
            _nav.ResetPath();

            if (defs[idx].needJump && forward)
            {
                AdvanceOnce();
                _nav.SetSpeed(moveSpeed);
                _nav.SetDestination(pts[idx], RigidNavigation.MoveMode.Jump);
                _lastAdvancePos = transform.position;
                _progressGateArmed = true;
                return;
            }

            if (defs[idx].dwellTime > 0f)
            {
                _waiting = true;
                _tWait = Time.time;
                return;
            }

            AdvanceOnce();
            _nav.SetSpeed(moveSpeed);
            _nav.SetDestination(pts[idx], mode);
            _lastAdvancePos = transform.position;
            _progressGateArmed = true;
            return;
        }

        FaceTowardsX(pts[idx].x);
    }

    private void FaceTowardsX(float tx)
    {
        float yaw = (tx - transform.position.x) >= 0f ? 90f : 270f;
        var target = Quaternion.Euler(0, yaw, 0);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, target, rotateDegPerSec * Time.deltaTime);
    }

    private bool HasArrived()
    {
        if (_progressGateArmed)
        {
            if (Vector3.Distance(transform.position, _lastAdvancePos) < minProgressAfterAdvance)
                return false;

            _progressGateArmed = false;
        }

        float rem = _nav.RemainingDistanceVector3();
        if (float.IsNaN(rem) || float.IsInfinity(rem))
            rem = Mathf.Abs(pts[idx].x - transform.position.x);

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
