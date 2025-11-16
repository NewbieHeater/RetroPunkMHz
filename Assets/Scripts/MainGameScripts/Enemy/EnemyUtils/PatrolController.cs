using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathProvider))]
public sealed class PatrolController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.8f;
    [SerializeField] float rotateDegPerSec = 180f;
    [SerializeField] bool pingPong = true;

    [SerializeField] PathProvider pathProvider;
    RigidNavigation nav;
    AnimatorProxy anim;

    int idx; bool forward = true; bool waiting; float tWait;
    IReadOnlyList<Vector3> pts;
    PatrolPoint[] defs;

    public void Init(RigidNavigation nav, AnimatorProxy anim)
    {
        this.nav = nav; this.anim = anim;
        defs = pathProvider.Definitions;
        pts = pathProvider.BuildWorldPoints(transform);
        pathProvider = GetComponent<PathProvider>();
    }

    public void Enter()
    {
        if (pts.Count == 0) return;
        idx = 0; forward = true; waiting = false;
        nav.SetSpeed(moveSpeed);
        anim.PlayMove();
        nav.SetDestinationWalk(pts[idx]);
    }

    public void Tick()
    {
        if (pts.Count == 0) return;

        // 1) 대기 먼저 처리 (대기 중엔 도착 체크 금지)
        if (waiting)
        {
            if (Time.time - tWait >= defs[idx].dwellTime)
            {
                waiting = false;
                Advance();
                nav.SetDestinationWalk(pts[idx]);
            }
            else
            {
                FaceTowardsX(pts[idx].x);
            }
            return; // ← 중요: 대기 중이면 아래 도착 체크를 타지 않게
        }

        // 2) 도착 체크 (대기 중이 아닐 때만)
        // 필요하면 x축 대신 거리로 바꾸세요:
        // bool arrived = Vector3.Distance(transform.position, pts[idx]) <= nav.stoppingDistance;
        bool arrived = Mathf.Abs(pts[idx].x - transform.position.x) < 0.05f;

        if (arrived)
        {
            nav.ResetPath();

            if (defs[idx].needJump && forward)
            {
                Advance();
                nav.SetDestinationJump(pts[idx]);
            }
            else if (defs[idx].dwellTime > 0f)
            {
                // 대기로 "처음 진입"하는 지점에서만 tWait 설정
                waiting = true;
                tWait = Time.time;
                return; // 바로 리턴하여 다음 프레임부터 대기 타이머만 진행
            }
            else
            {
                Advance();
                nav.SetDestinationWalk(pts[idx]);
            }
        }

        FaceTowardsX(pts[idx].x);
    }


    public void Exit() => nav.isStopped = true;

    void Advance()
    {
        int len = pts.Count;
        if (!pingPong) { idx = (idx + 1) % len; return; }

        if (forward) { idx++; if (idx >= len) { idx = len - 2; forward = false; } }
        else { idx--; if (idx < 0) { idx = 1; forward = true; } }
    }

    void FaceTowardsX(float tx)
    {
        float yaw = (tx - transform.position.x) > 0f ? 90f : 270f;
        var target = Quaternion.Euler(0, yaw, 0);
        anim.RotateTowards(target, rotateDegPerSec * Time.deltaTime);
    }
}
