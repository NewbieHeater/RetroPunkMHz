// LevelBrush Block Combiner (Collider-ready)
// - Keep your existing LevelBrushWindow workflow.
// - Post-process: merge placed 1x1 blocks into chunked combined meshes.
// - Adds **colliders** so the player can stand on merged terrain.
// - LevelPlane-aware (U/V/Normal). Groups by (chunkIndex, material).
//
// Usage:
//   Tools → Level Brush → Block Combiner
//   1) Assign LevelPlane, optional Parent root, set options.
//   2) Preview → Combine. Originals are disabled (or deleted if chosen).
//   3) You can Revert Last Combine (unless originals were deleted).

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum ColliderMode { None, Mesh, GreedyBoxes }
// -------------------------------------------------
// Marker on combined chunks (for revert)
// -------------------------------------------------
[ExecuteAlways]
public class CombinedChunk : MonoBehaviour
{
    [Tooltip("Chunk index in LevelPlane UV cell-space")] public Vector2Int chunkIndex;
    [Tooltip("LevelPlane grid size used for this batch")] public float gridSize;
    [Tooltip("Chunk size in cells")] public int chunkSize;
    [Tooltip("Original renderers that were disabled during combine")] public List<int> sourceRendererIDs = new();
    [Tooltip("Were originals destroyed instead of just disabled?")] public bool originalsDeleted;
    [Tooltip("Stamp to allow one-click revert for the last combine session")] public string sessionId;
    [Tooltip("Collider mode used")] public ColliderMode colliderMode;
    [Tooltip("Collider thickness along plane normal (for Box mode)")] public float colliderThickness;

    public IEnumerable<GameObject> ResolveSourceObjects()
    {
        foreach (var id in sourceRendererIDs)
        {
            var obj = EditorUtility.InstanceIDToObject(id);
            if (obj is GameObject go) yield return go;
            else if (obj is Component c) yield return c.gameObject;
        }
    }
}