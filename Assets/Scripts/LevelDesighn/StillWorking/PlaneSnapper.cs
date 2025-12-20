// Assets/Scripts/LevelEditing/PlaneSnapper.cs
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class PlaneSnapper : MonoBehaviour
{
    public LevelPlane plane;
    public bool snapWhileSelected = true;
    public bool lockNormalAxis = true; // 평면 밖(Z 등)을 0으로 고정
    public bool liveSnap = true;

    void LateUpdate()
    {
        if (!Application.isPlaying && liveSnap && plane)
        {
#if UNITY_EDITOR
            if (snapWhileSelected &&
                !UnityEditor.Selection.transforms.Contains(transform)) return;
#endif
            Vector3 p = transform.position;
            plane.ProjectToPlane(p, out var projected);
            var uv = plane.WorldToUV(projected);
            uv = plane.SnapUV(uv);
            var snapped = plane.UVToWorld(uv);

            if (lockNormalAxis) transform.position = snapped;
            else transform.position = new Vector3(snapped.x, snapped.y, snapped.z);
        }
    }
}
