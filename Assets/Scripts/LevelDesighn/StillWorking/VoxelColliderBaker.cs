using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 블록(격자)에 배치된 큐브들을 그리디하게 병합하여
/// 적은 수의 BoxCollider로 만드는 베이커.
/// 전제: 블록들은 축정렬(회전 없음), 동일한 격자 크기(cellSize).
/// </summary>
[DisallowMultipleComponent]
public class VoxelColliderBaker : MonoBehaviour
{
    [Header("블록 부모(자식들을 스캔)")]
    public Transform blocksRoot;

    [Header("결과 콜라이더를 달 위치(보통 이 컴포넌트가 붙은 오브젝트)")]
    public Transform targetRoot;

    [Header("격자 설정")]
    public Vector3 cellSize = Vector3.one;       // 각 블록 1칸의 월드 크기
    public Vector3 origin = Vector3.zero;      // 격자 원점(월드 기준)
    public float snapEps = 0.001f;            // 좌표 스냅 오차 허용

    [Header("대상 필터")]
    public bool includeInactive = true;
    public string requiredTag = "";              // 특정 태그만 병합하고 싶으면 지정

    [Header("기타 옵션")]
    public bool removeOriginalColliders = false; // 기존 개별 콜라이더 제거
    public bool clearPrevBaked = true;           // 이전에 만든 BoxCollider들 제거

    // 내부: 점유 셀
    struct Cell { public int x, y, z; }
    HashSet<(int x, int y, int z)> occupied;

    [ContextMenu("Bake BoxColliders (Greedy)")]
    public void Bake()
    {
        if (!blocksRoot) { Debug.LogWarning("blocksRoot 미지정"); return; }
        if (!targetRoot) targetRoot = this.transform;

        // 0) 이전 베이크 결과 정리
        if (clearPrevBaked)
        {
            foreach (var bc in targetRoot.GetComponentsInChildren<BoxCollider>(true))
            {
                if (bc.transform == targetRoot) DestroyImmediate(bc);
                else DestroyImmediate(bc.gameObject);
            }
        }

        // 1) 자식에서 블록 좌표 수집 → 격자 index 로 매핑
        occupied = new HashSet<(int, int, int)>();
        var blocks = blocksRoot.GetComponentsInChildren<Transform>(includeInactive);
        foreach (var t in blocks)
        {
            if (t == blocksRoot) continue;
            if (!string.IsNullOrEmpty(requiredTag) && !t.CompareTag(requiredTag)) continue;

            // 회전/스케일 없는 걸 전제로 권장 (있다면 먼저 정규화하세요)
            Vector3 wpos = t.position;

            // 월드 좌표 → 격자 인덱스
            var gx = Mathf.RoundToInt((wpos.x - origin.x) / Mathf.Max(cellSize.x, snapEps));
            var gy = Mathf.RoundToInt((wpos.y - origin.y) / Mathf.Max(cellSize.y, snapEps));
            var gz = Mathf.RoundToInt((wpos.z - origin.z) / Mathf.Max(cellSize.z, snapEps));

            occupied.Add((gx, gy, gz));
        }

        if (occupied.Count == 0)
        {
            Debug.Log("병합할 블록이 없습니다.");
            return;
        }

        // 2) 그리디 병합(3D): x→y→z 순서로 직육면체를 최대 확장
        var visited = new HashSet<(int, int, int)>();
        int made = 0;

        foreach (var cell in occupied)
        {
            if (visited.Contains(cell)) continue;

            // x 로 최대 확장
            int x0 = cell.x, y0 = cell.y, z0 = cell.z;
            int x1 = x0;
            while (occupied.Contains((x1 + 1, y0, z0)) && !visited.Contains((x1 + 1, y0, z0))) x1++;

            // y 로 최대 확장 (행이 모두 채워져 있어야 확장)
            int y1 = y0;
            bool canGrowY = true;
            while (canGrowY)
            {
                int ny = y1 + 1;
                for (int x = x0; x <= x1; x++)
                {
                    if (!occupied.Contains((x, ny, z0)) || visited.Contains((x, ny, z0)))
                    { canGrowY = false; break; }
                }
                if (canGrowY) y1 = ny;
            }

            // z 로 최대 확장 (슬랩 전체가 채워져 있어야 확장)
            int z1 = z0;
            bool canGrowZ = true;
            while (canGrowZ)
            {
                int nz = z1 + 1;
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        if (!occupied.Contains((x, y, nz)) || visited.Contains((x, y, nz)))
                        { canGrowZ = false; break; }
                    }
                if (canGrowZ) z1 = nz;
            }

            // 직육면체 범위 마킹
            for (int z = z0; z <= z1; z++)
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                        visited.Add((x, y, z));

            // 3) 결과 BoxCollider 생성 (targetRoot 로컬좌표)
            Vector3 size = new Vector3((x1 - x0 + 1) * cellSize.x,
                                        (y1 - y0 + 1) * cellSize.y,
                                        (z1 - z0 + 1) * cellSize.z);

            // 격자 중앙에서 로컬 중심점 계산
            Vector3 centerWorld =
                new Vector3((x0 + x1 + 1) * 0.5f * cellSize.x + origin.x,
                            (y0 + y1 + 1) * 0.5f * cellSize.y + origin.y,
                            (z0 + z1 + 1) * 0.5f * cellSize.z + origin.z);

            Vector3 centerLocal = targetRoot.worldToLocalMatrix.MultiplyPoint3x4(centerWorld);

            var bc = targetRoot.gameObject.AddComponent<BoxCollider>();
            bc.center = centerLocal - targetRoot.localPosition; // targetRoot 가 (0,0,0) 로컬이면 사실상 centerLocal
            bc.size = size;
            made++;
        }

        // 4) 원하면 기존 콜라이더 제거
        if (removeOriginalColliders)
        {
            foreach (var c in blocksRoot.GetComponentsInChildren<Collider>(includeInactive))
                if (c) DestroyImmediate(c);
        }

        Debug.Log($"VoxelColliderBaker: {occupied.Count} 블록 → {made}개의 BoxCollider로 병합 완료.");
    }
}
