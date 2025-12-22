using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class MathfExt
{
    public static int Mod(int a, int m) { int r = a % m; return r < 0 ? r + m : r; }
}



// -------------------------------------------------
// Main Editor Window
// -------------------------------------------------
public class LevelBrushCombinerWindow : EditorWindow
{
    const string MenuPath = "Tools/Level Brush/Block Combiner";
    const string DefaultCombinedRootName = "__CombinedBlocks";
    const string SkipTagName = "EditorOnly";     // ignore
    const string BrushTagName = "LevelBrush";    // legacy tag (not used when marker mode enabled)

    [MenuItem(MenuPath)]
    public static void Open() => GetWindow<LevelBrushCombinerWindow>("Block Combiner");

    [Header("Scene Inputs")]
    [SerializeField] LevelPlane levelPlane;
    [SerializeField] Transform limitToParent; // optional filter
    [SerializeField] Transform combinedRoot;  // where combined go

    [Header("Chunking & Filters")]
    [Min(0.01f)] public float gridSize = 1f;
    [Range(4, 64)] public int chunkSize = 16; // cells per side

    // Candidate filtering
    public bool onlyPlacedByBrushMarker = true; // if true => require LevelBrushPlaced
    public List<string> allowedTags = new List<string> { "Ground", "Wall" }; // combine only these tags
    public bool includeInactive = false;      // include disabled
    public bool skipIfHasRigidbody = true;    // skip dynamics
    public bool ignoreSkinned = true;         // skip skinned

    

    [Header("Combine Options")]
    public bool markStatic = true;            // mark static flags
    public bool deleteOriginals = false;      // irreversible
    public bool combinePerMaterial = true;    // per-material children
    public ColliderMode colliderMode = ColliderMode.GreedyBoxes;
    [Min(0f)] public float boxColliderThickness = 0.2f; // along normal (world units)

    [Header("Session")]
    [SerializeField] string lastSessionId = string.Empty;

    Vector2 _scroll;
    string _status = "Ready";

    void OnEnable()
    {
        if (!levelPlane)
            levelPlane = GameObject.FindFirstObjectByType<LevelPlane>(FindObjectsInactive.Include);

        if (!combinedRoot)
        {
            var go = GameObject.Find(DefaultCombinedRootName);
            if (!go)
            {
                go = new GameObject(DefaultCombinedRootName);
                Undo.RegisterCreatedObjectUndo(go, "Create Combined Root");
            }
            combinedRoot = go.transform;
        }

        if (levelPlane) gridSize = Mathf.Max(0.01f, levelPlane.gridSize);
    }

    void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        try
        {
            EditorGUILayout.LabelField("Scene Inputs", EditorStyles.boldLabel);
            levelPlane = (LevelPlane)EditorGUILayout.ObjectField("Level Plane", levelPlane, typeof(LevelPlane), true);
            limitToParent = (Transform)EditorGUILayout.ObjectField("Only Under Parent (optional)", limitToParent, typeof(Transform), true);
            combinedRoot = (Transform)EditorGUILayout.ObjectField("Combined Root", combinedRoot, typeof(Transform), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chunking & Filters", EditorStyles.boldLabel);
            gridSize = Mathf.Max(0.01f, EditorGUILayout.FloatField("Grid Size", gridSize));
            chunkSize = EditorGUILayout.IntSlider("Chunk Size (cells)", chunkSize, 4, 64);

            onlyPlacedByBrushMarker = EditorGUILayout.Toggle("Only Placed By Brush (Marker)", onlyPlacedByBrushMarker);
            includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);
            skipIfHasRigidbody = EditorGUILayout.Toggle("Skip if has Rigidbody", skipIfHasRigidbody);
            ignoreSkinned = EditorGUILayout.Toggle("Ignore SkinnedMesh", ignoreSkinned);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Allowed Tags To Combine", EditorStyles.boldLabel);
            for (int i = 0; i < allowedTags.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    allowedTags[i] = EditorGUILayout.TagField($"Tag {i + 1}", allowedTags[i]);
                    if (GUILayout.Button("-", GUILayout.Width(24)))
                    {
                        allowedTags.RemoveAt(i);
                        GUIUtility.ExitGUI();
                    }
                }
            }
            if (GUILayout.Button("Add Tag"))
                allowedTags.Add("Untagged");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combine Options", EditorStyles.boldLabel);
            combinePerMaterial = EditorGUILayout.Toggle("Combine Per Material", combinePerMaterial);
            markStatic = EditorGUILayout.Toggle("Mark Combined Static", markStatic);
            deleteOriginals = EditorGUILayout.ToggleLeft("Delete Originals (IRREVERSIBLE)", deleteOriginals);
            colliderMode = (ColliderMode)EditorGUILayout.EnumPopup("Collider Mode", colliderMode);
            if (colliderMode == ColliderMode.GreedyBoxes)
                boxColliderThickness = EditorGUILayout.FloatField("Box Thickness (world)", Mathf.Max(0f, boxColliderThickness));

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Candidates", GUILayout.Height(28))) TryReportCounts();
                if (GUILayout.Button("Combine", GUILayout.Height(28)))
                {
                    try
                    {
                        CombineNow();
                    }
                    catch (Exception ex)
                    {
                        _status = "Combine failed. See Console for details.";
                        Debug.LogException(ex);
                    }
                }
            }

            if (GUILayout.Button("Revert Last Combine (by Session)", GUILayout.Height(24)))
                RevertLast();

            EditorGUILayout.HelpBox(_status, MessageType.Info);
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }

    struct Candidate
    {
        public Renderer renderer;
        public MeshFilter mf;
        public Mesh mesh;
        public Transform t;

        public Vector2Int chunkIdx;
        public Vector2Int cell;
        public Vector2Int localCell;

        // Root object to disable/delete (marker root if exists, else renderer GO)
        public GameObject sourceRoot;
    }

    void TryReportCounts()
    {
        if (!levelPlane) { _status = "Assign LevelPlane"; return; }
        var cands = CollectCandidates();
        int total = cands.Count;
        int chunkCount = cands.Select(c => c.chunkIdx).Distinct().Count();
        _status = $"Candidates: {total} renderers ¡æ {chunkCount} chunks";
    }

    List<Candidate> CollectCandidates()
    {
        var list = new List<Candidate>(1024);

        var renderers = includeInactive
            ? GameObject.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            : GameObject.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var r in renderers)
        {
            if (!r) continue;
            if (r is ParticleSystemRenderer) continue;
            if (ignoreSkinned && r is SkinnedMeshRenderer) continue;

            // Marker-based filtering
            var placed = r.GetComponentInParent<LevelBrushPlaced>();
            if (onlyPlacedByBrushMarker)
            {
                if (!placed) continue;
                if (!placed.allowCombine) continue;
            }

            // Tag filtering: use marker root if present (more stable)
            GameObject tagGO = placed ? placed.gameObject : r.gameObject;

            if (allowedTags != null && allowedTags.Count > 0)
            {
                bool ok = false;
                foreach (var ttag in allowedTags)
                {
                    if (!string.IsNullOrEmpty(ttag) && tagGO.CompareTag(ttag)) { ok = true; break; }
                }
                if (!ok) continue;
            }

            if (r.CompareTag(SkipTagName)) continue;
            if (skipIfHasRigidbody && r.GetComponentInParent<Rigidbody>()) continue;
            if (limitToParent && !r.transform.IsChildOf(limitToParent)) continue;
            if (r.GetComponentInParent<CombinedChunk>()) continue; // already combined

            var mf = r.GetComponent<MeshFilter>();
            if (!mf || !mf.sharedMesh) continue;

            if (!levelPlane.ProjectToPlane(r.transform.position, out var onPlane)) continue;

            var uv = levelPlane.WorldToUV(onPlane);
            var cell = new Vector2Int(Mathf.FloorToInt(uv.x / gridSize), Mathf.FloorToInt(uv.y / gridSize));
            var cidx = new Vector2Int(Mathf.FloorToInt((float)cell.x / chunkSize), Mathf.FloorToInt((float)cell.y / chunkSize));
            var local = new Vector2Int(MathfExt.Mod(cell.x, chunkSize), MathfExt.Mod(cell.y, chunkSize));

            list.Add(new Candidate
            {
                renderer = r,
                mf = mf,
                mesh = mf.sharedMesh,
                t = r.transform,

                chunkIdx = cidx,
                cell = cell,
                localCell = local,

                sourceRoot = placed ? placed.gameObject : r.gameObject
            });
        }

        return list;
    }

    void CombineNow()
    {
        if (!levelPlane) { _status = "Assign LevelPlane"; return; }
        if (!combinedRoot) { _status = "Assign Combined Root"; return; }

        // Layer safety (Ground may not exist)
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0) groundLayer = 0;

        var sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        lastSessionId = sessionId;

        // Collect + materialize to avoid LINQ deferred execution issues
        var cands = CollectCandidates()
            .Where(c => c.renderer && c.mf && c.mesh && c.t)
            .ToList();

        if (cands.Count == 0) { _status = "No valid candidates."; return; }

        // IMPORTANT: ToList() to avoid deferred execution invalidating UnityEngine.Object refs
        var byChunk = cands
            .GroupBy(c => c.chunkIdx)
            .ToList();

        int chunkMade = 0;
        int totalUsed = 0;
        int totalBoxes = 0;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("LevelBrush Combine");

        foreach (var g0 in byChunk)
        {
            // Freeze group and re-validate in case refs got invalidated between steps
            var chunkItems = g0
                .Where(c => c.renderer && c.mf && c.mesh && c.t)
                .ToList();

            if (chunkItems.Count == 0) continue;

            var chunkGO = new GameObject($"CombinedChunk {g0.Key.x},{g0.Key.y}");
            chunkGO.layer = groundLayer;
            Undo.RegisterCreatedObjectUndo(chunkGO, "Create Combined Chunk");
            chunkGO.transform.SetParent(combinedRoot, true);

            Vector3 origin = levelPlane.Origin
                + levelPlane.U * (g0.Key.x * chunkSize * gridSize)
                + levelPlane.V * (g0.Key.y * chunkSize * gridSize);

            chunkGO.transform.position = origin;

            // LookRotation can throw/behave oddly if vectors are zero; still not NRE but validate anyway
            var n = levelPlane.Normal;
            var up = levelPlane.V;
            if (n == Vector3.zero) n = Vector3.forward;
            if (up == Vector3.zero) up = Vector3.up;
            chunkGO.transform.rotation = Quaternion.LookRotation(n, up);

            var marker = chunkGO.AddComponent<CombinedChunk>();
            marker.chunkIndex = g0.Key;
            marker.gridSize = gridSize;
            marker.chunkSize = chunkSize;
            marker.sessionId = sessionId;
            marker.originalsDeleted = deleteOriginals;
            marker.colliderMode = colliderMode;
            marker.colliderThickness = boxColliderThickness;

            // ---------- Combine Meshes ----------
            IEnumerable<IGrouping<Material, Candidate>> groups;
            if (combinePerMaterial)
            {
                // Materialize group list to avoid deferred enumeration issues
                groups = chunkItems
                    .Where(c => c.renderer)
                    .GroupBy(c => c.renderer.sharedMaterial) // can be null; that's fine
                    .ToList();
            }
            else
            {
                // Single group
                groups = new[] { chunkItems.GroupBy(_ => (Material)null).First() };
            }

            int usedInChunk = 0;

            foreach (var matGroup in groups)
            {
                var material = combinePerMaterial ? matGroup.Key : null;
                var matItems = matGroup
                    .Where(c => c.renderer && c.mf && c.mesh)
                    .ToList();

                if (matItems.Count == 0) continue;

                var ciList = new List<CombineInstance>(matItems.Count);

                foreach (var c in matItems)
                {
                    var r = c.renderer;
                    var mf = c.mf;
                    var mesh = c.mesh;

                    if (!r || !mf || !mesh) continue;

                    // sharedMaterials can be null
                    var mats = r.sharedMaterials;

                    if (combinePerMaterial && mats != null && mats.Length > 1)
                    {
                        for (int sub = 0; sub < mesh.subMeshCount; sub++)
                        {
                            var matAtSub = (sub < mats.Length) ? mats[sub] : r.sharedMaterial;
                            if (matAtSub != material) continue;

                            ciList.Add(new CombineInstance
                            {
                                mesh = mesh,
                                subMeshIndex = sub,
                                transform = mf.transform.localToWorldMatrix
                            });
                            usedInChunk++;
                        }
                    }
                    else
                    {
                        ciList.Add(new CombineInstance
                        {
                            mesh = mesh,
                            subMeshIndex = 0,
                            transform = mf.transform.localToWorldMatrix
                        });
                        usedInChunk++;
                    }

                    // Store original reference for revert (store root IDs is better, but keep existing behavior)
                    marker.sourceRendererIDs.Add(r.GetInstanceID());
                }

                if (ciList.Count == 0) continue;

                var combinedMesh = new Mesh
                {
                    name = $"cmb_{g0.Key.x}_{g0.Key.y}_{(material ? material.name : "all")}"
                };

                // Convert world matrices into chunk local space
                var inv = chunkGO.transform.worldToLocalMatrix;
                for (int i = 0; i < ciList.Count; i++)
                {
                    var ci = ciList[i];
                    ci.transform = inv * ci.transform;
                    ciList[i] = ci;
                }

                combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combinedMesh.CombineMeshes(ciList.ToArray(), true, true, false);

                var child = new GameObject(material ? $"MR_{material.name}" : "MR_all");
                child.layer = groundLayer;
                Undo.RegisterCreatedObjectUndo(child, "Create Combined MR");
                child.transform.SetParent(chunkGO.transform, false);

                var mfNew = child.AddComponent<MeshFilter>();
                var mrNew = child.AddComponent<MeshRenderer>();

                mfNew.sharedMesh = combinedMesh;

                if (combinePerMaterial)
                {
                    mrNew.sharedMaterial = material; // can be null -> uses default
                }
                else
                {
                    mrNew.sharedMaterials = GatherAllMaterials(matItems);
                }

                // Collider attachment
                if (colliderMode == ColliderMode.Mesh)
                {
                    var mc = child.AddComponent<MeshCollider>();
                    mc.sharedMesh = combinedMesh; // non-convex OK for static terrain
                }
            }

            // ---------- Greedy Box Colliders ----------
            if (colliderMode == ColliderMode.GreedyBoxes)
            {
                var occ = new bool[chunkSize, chunkSize];
                foreach (var c in chunkItems)
                {
                    if ((uint)c.localCell.x < chunkSize && (uint)c.localCell.y < chunkSize)
                        occ[c.localCell.x, c.localCell.y] = true;
                }
                totalBoxes += BuildGreedyBoxes(chunkGO, occ, gridSize, boxColliderThickness);
            }

            if (markStatic)
            {
                GameObjectUtility.SetStaticEditorFlags(chunkGO,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.ContributeGI);
            }

            // Disable or delete originals (unique roots)
            var uniqueRoots = new HashSet<GameObject>();
            foreach (var c in chunkItems)
                if (c.sourceRoot) uniqueRoots.Add(c.sourceRoot);

            foreach (var root in uniqueRoots)
            {
                if (!root) continue;

                if (deleteOriginals)
                {
                    Undo.DestroyObjectImmediate(root);
                }
                else
                {
                    Undo.RecordObject(root, "Disable Original Block");
                    root.SetActive(false);
                }
            }

            chunkMade++;
            totalUsed += usedInChunk;
        }

        Undo.CollapseUndoOperations(group);

        _status =
            $"Combined {totalUsed} instances into {chunkMade} chunks. " +
            $"Colliders: {(colliderMode == ColliderMode.Mesh ? "Mesh" : colliderMode == ColliderMode.GreedyBoxes ? "Boxes" : "None")} " +
            $"(boxes={totalBoxes}). Session={lastSessionId}";
    }

    static Material[] GatherAllMaterials(IEnumerable<Candidate> group)
    {
        var set = new HashSet<Material>();

        foreach (var c in group)
        {
            if (!c.renderer) continue;

            var mats = c.renderer.sharedMaterials;
            if (mats != null && mats.Length > 0)
            {
                foreach (var m in mats)
                    if (m) set.Add(m);
            }
            else
            {
                var sm = c.renderer.sharedMaterial;
                if (sm) set.Add(sm);
            }
        }

        return set.ToArray();
    }

    // Build maximal rectangles of occupied cells -> BoxColliders aligned to chunk local X/Y and thickness on Z
    static int BuildGreedyBoxes(GameObject chunkGO, bool[,] occ, float grid, float thickness)
    {
        int W = occ.GetLength(0), H = occ.GetLength(1);
        var used = new bool[W, H];
        int count = 0;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                if (!occ[x, y] || used[x, y]) continue;

                // Expand width
                int w = 1;
                while (x + w < W && occ[x + w, y] && !used[x + w, y]) w++;

                // Expand height while preserving width
                int h = 1;
                bool ok = true;
                while (y + h < H && ok)
                {
                    for (int i = 0; i < w; i++)
                    {
                        if (!occ[x + i, y + h] || used[x + i, y + h]) { ok = false; break; }
                    }
                    if (ok) h++;
                }

                // Mark used
                for (int yy = 0; yy < h; yy++)
                    for (int xx = 0; xx < w; xx++)
                        used[x + xx, y + yy] = true;

                var bc = chunkGO.AddComponent<BoxCollider>();

                // Center in local chunk space: lower-left (0,0) at chunk origin
                float cx = (x + w * 0.5f - 0.5f) * grid;
                float cy = (y + h * 0.5f - 0.5f) * grid;

                bc.center = new Vector3(cx, cy, 0f);
                bc.size = new Vector3(w * grid, h * grid, Mathf.Max(0.001f, thickness));
                count++;
            }
        }

        return count;
    }

    void RevertLast()
    {
        if (string.IsNullOrEmpty(lastSessionId)) { _status = "No last session id"; return; }

        var allChunks = GameObject.FindObjectsByType<CombinedChunk>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var targets = allChunks.Where(c => c && c.sessionId == lastSessionId).ToList();

        if (targets.Count == 0) { _status = "No chunks matching last session"; return; }

        Undo.IncrementCurrentGroup();
        int g = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Revert Combined Chunks");

        int reEnabled = 0;

        foreach (var ch in targets)
        {
            if (!ch.originalsDeleted)
            {
                foreach (var go in ch.ResolveSourceObjects())
                {
                    if (!go) continue;
                    if (!go.activeSelf)
                    {
                        Undo.RecordObject(go, "Enable Original");
                        go.SetActive(true);
                        reEnabled++;
                    }
                }
            }

            Undo.DestroyObjectImmediate(ch.gameObject);
        }

        Undo.CollapseUndoOperations(g);
        _status = $"Reverted {targets.Count} chunks; re-enabled {reEnabled} originals";
    }
}
