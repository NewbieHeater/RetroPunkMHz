// Assets/Scripts/LevelEditing/LevelPlane.cs
using UnityEngine;

[ExecuteAlways]
public class LevelPlane : MonoBehaviour
{
    [Header("Plane Definition")]
    public Vector3 planeNormal = Vector3.forward; // 예: Z 고정(2.5D 흔한 케이스)
    [Tooltip("평면 위 첫 번째 축 (U). Normal과 수직이어야 함")]
    public Vector3 axisU = Vector3.right;
    public float gridSize = 1f;

    [Header("Grid Preview")]
    public bool enableGrid = false;
    [Tooltip("그리드 가로 칸 수 (U 방향)")]
    public int gridCountX = 40;
    [Tooltip("그리드 세로 칸 수 (V 방향)")]
    public int gridCountY = 25;
    public Color gizmoColor = new Color(0, 1, 1, 0.35f);

    public Vector3 Origin => transform.position;
    public Vector3 Normal => transform.TransformDirection(planeNormal).normalized;
    public Vector3 U => Vector3.ProjectOnPlane(transform.TransformDirection(axisU), Normal).normalized;
    public Vector3 V
    {
        get
        {
            var v = Vector3.Cross(Normal, U).normalized;
            return v.sqrMagnitude < 1e-6f ? Vector3.up : v;
        }
    }

    public bool ProjectToPlane(Vector3 world, out Vector3 projected)
    {
        var n = Normal;
        var toPoint = world - Origin;
        var dist = Vector3.Dot(toPoint, n);
        projected = world - dist * n;
        return true;
    }

    public Vector2 WorldToUV(Vector3 worldOnPlane)
    {
        var d = worldOnPlane - Origin;
        float u = Vector3.Dot(d, U);
        float v = Vector3.Dot(d, V);
        return new Vector2(u, v);
    }

    public Vector3 UVToWorld(Vector2 uv)
    {
        return Origin + U * uv.x + V * uv.y;
    }

    public Vector2 SnapUV(Vector2 uv)
    {
        if (gridSize <= 0f) return uv;
        return new Vector2(Mathf.Round(uv.x / gridSize) * gridSize,
                           Mathf.Round(uv.y / gridSize) * gridSize);
    }

    private void OnDrawGizmos()
    {
        if (!enableGrid) return;
        Gizmos.color = gizmoColor;

        int halfX = gridCountX / 2;
        int halfY = gridCountY / 2;

        // 세로선 (U 방향으로)
        for (int x = -halfX; x <= halfX; x++)
        {
            Vector3 a = UVToWorld(new Vector2(x * gridSize, -halfY * gridSize));
            Vector3 b = UVToWorld(new Vector2(x * gridSize, halfY * gridSize));
            Gizmos.DrawLine(a, b);
        }

        // 가로선 (V 방향으로)
        for (int y = -halfY; y <= halfY; y++)
        {
            Vector3 c = UVToWorld(new Vector2(-halfX * gridSize, y * gridSize));
            Vector3 d = UVToWorld(new Vector2(halfX * gridSize, y * gridSize));
            Gizmos.DrawLine(c, d);
        }
    }
}
