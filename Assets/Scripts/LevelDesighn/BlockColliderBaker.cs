using UnityEngine;
using System.Collections.Generic;

public class BlockColliderBaker : MonoBehaviour
{
    [Header("블록들이 달린 부모")]
    public Transform blocksRoot;

    [Header("베이크 결과를 달 대상")]
    public MeshCollider targetCollider;   // 새로 붙여도 됨 (비Convex, Rigidbody 금지)

    [Header("옵션")]
    public bool includeInactive = true;
    public bool removeOriginalColliders = false; // 베이크 후 원래 콜라이더 제거(선택)

    [ContextMenu("Bake Now")]
    public void BakeNow()
    {
        if (!blocksRoot) { Debug.LogWarning("blocksRoot 미지정"); return; }
        if (!targetCollider) targetCollider = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();

        var mfs = blocksRoot.GetComponentsInChildren<MeshFilter>(includeInactive);
        var combines = new List<CombineInstance>(mfs.Length);

        foreach (var mf in mfs)
        {
            if (!mf || !mf.sharedMesh) continue;

            var ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            combines.Add(ci);
        }

        if (combines.Count == 0) { Debug.LogWarning("결합할 MeshFilter가 없습니다."); return; }

        // 새 메시 생성
        var combined = new Mesh { name = "BlocksCombinedColliderMesh" };
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 큰 메시 대응
        combined.CombineMeshes(combines.ToArray(), true, true, false);

        // 콜라이더 적용 (비Convex, Rigidbody 없음 권장)
        targetCollider.sharedMesh = null;          // 교체 시 먼저 null
        targetCollider.convex = false;             // 큰 지형은 non-convex
        targetCollider.sharedMesh = combined;      // 적용

        if (removeOriginalColliders)
        {
            var cols = blocksRoot.GetComponentsInChildren<Collider>(includeInactive);
            foreach (var c in cols) if (c && c.gameObject != targetCollider.gameObject) Destroy(c);
        }

        Debug.Log($"Baked MeshCollider from {combines.Count} meshes → {combined.vertexCount} verts.");
    }

    void Update()
    {
        // 예시: G 키로 베이크
        if (Input.GetKeyDown(KeyCode.G)) BakeNow();
    }
}
