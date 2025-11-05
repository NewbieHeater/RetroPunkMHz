using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class BlockColliderBaker : MonoBehaviour
{
    [Header("블록들이 달린 부모")]
    public Transform blocksRoot;

    [Header("베이크 결과를 달 대상")]
    public MeshCollider targetCollider;   // 새로 붙여도 됨 (비Convex, Rigidbody 금지)

    [Header("옵션")]
    public bool includeInactive = true;
    public bool removeOriginalColliders = false; // 베이크 후 원래 콜라이더 제거(선택)
    [Range(0f, 0.01f)]
    public float inflateAmount = 0.001f;  // 블록 메시 팽창 정도 (빈틈 방지)

    [ContextMenu("Bake Now")]
    public void BakeNow()
    {
        if (!blocksRoot)
        {
            Debug.LogWarning("blocksRoot 미지정");
            return;
        }

        // 타겟 콜라이더 확보
        if (!targetCollider)
            targetCollider = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();

        var mfs = blocksRoot.GetComponentsInChildren<MeshFilter>(includeInactive);
        var combines = new List<CombineInstance>(mfs.Length);

        foreach (var mf in mfs)
        {
            if (!mf || !mf.sharedMesh) continue;

            // 블록 메시 팽창 (틈 방지)
            Mesh inflatedMesh = InflateMesh(mf.sharedMesh, inflateAmount);

            var ci = new CombineInstance
            {
                mesh = inflatedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            combines.Add(ci);
        }

        if (combines.Count == 0)
        {
            Debug.LogWarning("결합할 MeshFilter가 없습니다.");
            return;
        }

        // 새 메시 생성
        var combined = new Mesh { name = "BlocksCombinedColliderMesh" };
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 큰 메시 대응
        combined.CombineMeshes(combines.ToArray(), true, true, false);
        combined.RecalculateNormals();

        // 콜라이더 적용 (비Convex, Rigidbody 없음 권장)
        targetCollider.sharedMesh = null;          // 교체 시 먼저 null
        targetCollider.convex = false;             // 큰 지형은 non-convex
        targetCollider.sharedMesh = combined;      // 적용

        // 원본 콜라이더 제거 옵션
        if (removeOriginalColliders)
        {
            var cols = blocksRoot.GetComponentsInChildren<Collider>(includeInactive);
            foreach (var c in cols)
                if (c && c.gameObject != targetCollider.gameObject)
                    DestroyImmediate(c);
        }

        Debug.Log($"[BlockColliderBaker] Baked {combines.Count} meshes → {combined.vertexCount} verts. Inflate: {inflateAmount}");
    }

    /// <summary>
    /// 메시를 법선 방향으로 미세하게 확장 (틈 방지용)
    /// </summary>
    Mesh InflateMesh(Mesh mesh, float amount)
    {
        if (amount <= 0f) return mesh;

        Mesh m = new Mesh();
        var verts = mesh.vertices;
        var norms = mesh.normals;

        if (norms == null || norms.Length != verts.Length)
        {
            mesh.RecalculateNormals();
            norms = mesh.normals;
        }

        var newVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            newVerts[i] = verts[i] + norms[i] * amount;

        m.vertices = newVerts;
        m.triangles = mesh.triangles;
        m.RecalculateBounds();
        m.RecalculateNormals();
        return m;
    }

    void Update()
    {
        // 예시: G 키로 베이크
        if (Application.isPlaying && Input.GetKeyDown(KeyCode.G))
            BakeNow();
    }
}
