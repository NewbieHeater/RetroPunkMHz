using System.Collections.Generic;
using UnityEngine;

public sealed class PathProvider : MonoBehaviour
{
    [SerializeField] PatrolPoint[] patrolPoints;
    public PatrolPoint[] Definitions => patrolPoints;

    public IReadOnlyList<Vector3> BuildWorldPoints(Transform owner)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return System.Array.Empty<Vector3>();

        var outPts = new Vector3[patrolPoints.Length];
        Vector3 acc = owner.position + owner.TransformDirection(patrolPoints[0].relativeMovePoint);
        outPts[0] = acc;
        for (int i = 1; i < patrolPoints.Length; i++)
        {
            acc += owner.TransformDirection(patrolPoints[i].relativeMovePoint);
            outPts[i] = acc;
        }
        return outPts;
    }

    // 에디터 확인용 기즈모
    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        var pts = BuildWorldPoints(transform);
        Gizmos.color = Color.cyan;
        for (int i = 0; i < pts.Count; i++)
        {
            Gizmos.DrawSphere(pts[i], 0.08f);
            if (i + 1 < pts.Count) Gizmos.DrawLine(pts[i], pts[i + 1]);
        }
    }
}
