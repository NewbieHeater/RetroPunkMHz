using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelBrushWindow : EditorWindow
{
    private LevelPlane _plane;
    private PrefabPalette _palette;

    // 선택 상태
    private GameObject _activePrefab;
    private Transform _parent;
    private int _rotationSteps; // 0,1,2,3 => 0/90/180/270
    private int _brushSize = 1;

    // 탐색/필터
    private string _search = "";
    private string _tagFilter = "All";
    private Vector2 _mainScroll;    // 전체 창 스크롤
    private Vector2 _paletteScroll; // 팔레트 영역 전용 스크롤

    // 즐겨찾기/최근/핫바
    private const int HotbarSize = 9;
    private GameObject[] _hotbar = new GameObject[HotbarSize];
    private List<GameObject> _recent = new List<GameObject>();
    private const int RecentMax = 12;

    // 라인 페인트
    private bool _lineModeActive;
    private Vector2? _lineStartUV;

    // UI 스타일
    private GUIStyle _tagChipStyle;

    // 마우스 반응성
    private bool _forceSceneMouseMove = true;

    // 브러시 활성/비활성 토글
    private bool _brushEnabled = true;

    private const string _planeRefKey = "LevelBrush.LastPlaneGOID";
    private const string BRUSH_TAG = "LevelBrush";
    private static readonly Collider[] _eraseBuf = new Collider[128];

    [MenuItem("Tools/Level Brush")]
    public static void ShowWindow() => GetWindow<LevelBrushWindow>("Level Brush");

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        // 씬뷰가 마우스 이동 이벤트를 계속 받도록
        EditorApplication.delayCall += () =>
        {
            foreach (SceneView sv in SceneView.sceneViews)
                if (sv) sv.wantsMouseMove = true;
        };

        TryResolvePlane(autoCreateIfMissing: false);
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        foreach (SceneView sv in SceneView.sceneViews)
            if (sv) sv.wantsMouseMove = false;
    }

    void InitStyles()
    {
        if (_tagChipStyle == null)
        {
            _tagChipStyle = new GUIStyle(EditorStyles.miniButton)
            {
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(8, 8, 4, 4)
            };
        }
    }


void TryResolvePlane(bool autoCreateIfMissing = true)
    {
        // 1) 저장된 GlobalObjectId로 복원 시도
#if UNITY_EDITOR
        if (_plane == null)
        {
            string saved = EditorPrefs.GetString(_planeRefKey, string.Empty);
            if (!string.IsNullOrEmpty(saved) && GlobalObjectId.TryParse(saved, out var goid))
            {
                Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(goid);
                if (obj is LevelPlane lp) _plane = lp;
            }
        }
#endif

        // 2) 씬에서 자동 탐색
        if (_plane == null)
        {
            _plane = Object.FindFirstObjectByType<LevelPlane>(FindObjectsInactive.Include);
            // (구버전에선 FindObjectOfType<LevelPlane>(true) 사용)
        }

        // 3) 필요 시 자동 생성
        if (_plane == null && autoCreateIfMissing)
        {
            var go = new GameObject("__LevelPlane");
            _plane = go.AddComponent<LevelPlane>();
            Undo.RegisterCreatedObjectUndo(go, "Create LevelPlane");
            // 기본값 살짝 가다듬기 (2.5D 흔한 Z-normal)
            _plane.planeNormal = Vector3.forward;
            _plane.axisU = Vector3.right;
            _plane.gridSize = 1f;
        }

        // 4) 잡혔다면 저장
#if UNITY_EDITOR
        if (_plane != null)
        {
            var goid = GlobalObjectId.GetGlobalObjectIdSlow(_plane);
            EditorPrefs.SetString(_planeRefKey, goid.ToString());
        }
#endif
    }

    void SavePlaneRef()
    {
#if UNITY_EDITOR
        if (_plane == null) { EditorPrefs.DeleteKey(_planeRefKey); return; }
        var goid = GlobalObjectId.GetGlobalObjectIdSlow(_plane);
        EditorPrefs.SetString(_planeRefKey, goid.ToString());
#endif
    }

    void OnGUI()
    {
        InitStyles();


        _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);

        EditorGUI.BeginChangeCheck();
        _plane = (LevelPlane)EditorGUILayout.ObjectField("Level Plane", _plane, typeof(LevelPlane), true);
        if (EditorGUI.EndChangeCheck())
            SavePlaneRef();

        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Find In Scene", GUILayout.Width(120)))
                {
                    _plane = Object.FindFirstObjectByType<LevelPlane>(FindObjectsInactive.Include);
                    SavePlaneRef();
                }
                if (GUILayout.Button("Create One", GUILayout.Width(120)))
                {
                    TryResolvePlane(autoCreateIfMissing: true);
                }
            }
            //if (GUI.changed) SavePlaneRef();

            // 상단 상태바: 활성/비활성 토글 + 단축키 안내
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                _brushEnabled = GUILayout.Toggle(
                    _brushEnabled,
                    _brushEnabled ? "브러시 활성(B)" : "브러시 비활성(B)",
                    "Button",
                    GUILayout.Height(25),
                    GUILayout.Width(200)
                );
                GUILayout.Label("단축키: B 토글, 1~9 핫바, Q/E 순환, I 아이드랍퍼, Alt+클릭 픽업, Shift+클릭 직선 색칠", GUILayout.ExpandWidth(false));
            }

            
            //_plane = (LevelPlane)EditorGUILayout.ObjectField("Level Plane", _plane, typeof(LevelPlane), true);
            _palette = (PrefabPalette)EditorGUILayout.ObjectField("Prefab Palette", _palette, typeof(PrefabPalette), false);
            _parent = (Transform)EditorGUILayout.ObjectField("Parent (optional)", _parent, typeof(Transform), true);
            _brushSize = EditorGUILayout.IntSlider("Brush Size (cells)", _brushSize, 1, 7);

            // 액션 바
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("회전 R", GUILayout.Width(70))) _rotationSteps = (_rotationSteps + 1) % 4;
                if (GUILayout.Button("지우개 토글(Tab)", GUILayout.Width(120))) _eraseMode = !_eraseMode;
                GUILayout.Label("배치/삭제는 SceneView에서 동작합니다.");
            }

            EditorGUILayout.Space(4);
            DrawSearchAndTags();
            EditorGUILayout.Space(4);
            DrawHotbar();
            EditorGUILayout.Space(6);
            DrawPaletteGrid();
            EditorGUILayout.Space(8);
            DrawRecentStrip();
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawSearchAndTags()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.SetNextControlName("searchField");
            _search = EditorGUILayout.TextField("검색", _search);
            if (GUILayout.Button("지우기", GUILayout.Width(60))) _search = string.Empty;
        }

        var tags = new HashSet<string> { "All" };
        if (_palette && _palette.items != null)
            foreach (var it in _palette.items)
                if (!string.IsNullOrEmpty(it.tag)) tags.Add(it.tag);

        using (new EditorGUILayout.HorizontalScope())
        {
            foreach (var t in tags)
            {
                bool on = (_tagFilter == t);
                if (GUILayout.Toggle(on, new GUIContent(t), _tagChipStyle) != on)
                    _tagFilter = t;
            }
        }
    }

    void DrawHotbar()
    {
        EditorGUILayout.LabelField("핫바(1~9)");
        using (new EditorGUILayout.HorizontalScope())
        {
            for (int i = 0; i < HotbarSize; i++)
            {
                var p = _hotbar[i];
                var label = (i + 1).ToString();
                Texture tex = AssetPreview.GetAssetPreview(p) ?? AssetPreview.GetMiniThumbnail(p);
                var content = new GUIContent(tex, p ? p.name : "빈 슬롯: 드래그로 할당");
                var rect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(56), GUILayout.Height(56));
                if (GUI.Button(rect, content)) { if (p) SelectPrefab(p); }

                // 드래그 할당
                if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) && rect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is GameObject go) { _hotbar[i] = go; Repaint(); break; }
                        }
                    }
                    Event.current.Use();
                }

                // 번호 라벨
                var lr = rect; lr.x += 4; lr.y += 4; lr.width = 20; lr.height = 16;
                GUI.Label(lr, label, EditorStyles.boldLabel);
            }
        }
    }

    void DrawPaletteGrid()
    {
        if (!_palette) { EditorGUILayout.HelpBox("Prefab Palette을 지정하세요.", MessageType.Info); return; }
        var list = FilteredItems();

        int col = Mathf.Max(3, (int)(position.width / 90f));
        _paletteScroll = EditorGUILayout.BeginScrollView(_paletteScroll, GUILayout.Height(220));
        int i = 0;
        while (i < list.Count)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < col && i < list.Count; c++, i++)
            {
                var it = list[i];
                var prefab = it.prefab;
                var icon = it.customIcon ? it.customIcon : (AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab));
                using (new EditorGUILayout.VerticalScope("box", GUILayout.Width(80)))
                {
                    if (GUILayout.Button(icon, GUILayout.Width(70), GUILayout.Height(56))) SelectPrefab(prefab);
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(it.label) ? prefab.name : it.label, EditorStyles.miniLabel, GUILayout.Width(72));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        it.favorite = GUILayout.Toggle(it.favorite, "★", "Button", GUILayout.Width(24));
                        if (EditorGUILayout.DropdownButton(new GUIContent("↦", "핫바로"), FocusType.Passive, GUILayout.Width(24)))
                        {
                            var menu = new GenericMenu();
                            for (int slot = 0; slot < HotbarSize; slot++)
                            {
                                int ix = slot;
                                menu.AddItem(new GUIContent($"핫바 {slot + 1}"), false, () => { _hotbar[ix] = prefab; Repaint(); });
                            }
                            menu.ShowAsContext();
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawRecentStrip()
    {
        if (_recent.Count == 0) return;
        EditorGUILayout.LabelField("최근 사용");
        using (new EditorGUILayout.HorizontalScope())
        {
            foreach (var p in _recent)
            {
                var tex = AssetPreview.GetAssetPreview(p) ?? AssetPreview.GetMiniThumbnail(p);
                if (GUILayout.Button(new GUIContent(tex, p.name), GUILayout.Width(42), GUILayout.Height(42))) SelectPrefab(p);
            }
        }
    }

    List<PrefabPalette.Entry> FilteredItems()
    {
        var items = _palette.items.Where(e => e.prefab).ToList();
        if (!string.IsNullOrEmpty(_search))
            items = items.Where(e => (e.label ?? "").ToLower().Contains(_search.ToLower()) || e.prefab.name.ToLower().Contains(_search.ToLower())).ToList();
        if (_tagFilter != "All")
            items = items.Where(e => e.tag == _tagFilter).ToList();
        items = items.OrderByDescending(e => e.favorite).ThenBy(e => string.IsNullOrEmpty(e.label) ? e.prefab.name : e.label).ToList();
        return items;
    }

    void SelectPrefab(GameObject p)
    {
        _activePrefab = p;
        _recent.Remove(p);
        _recent.Insert(0, p);
        if (_recent.Count > RecentMax) _recent.RemoveAt(_recent.Count - 1);
        Repaint();
    }

    // ====== Scene GUI ======
    private bool _eraseMode;

    void OnSceneGUI(SceneView sv)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B)
        {
            _brushEnabled = !_brushEnabled;
            e.Use();
            sv.Repaint();
        }

        if (!_brushEnabled) return;
        if (_plane == null) return;

        if (_forceSceneMouseMove)
        {
            sv.wantsMouseMove = true;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                sv.Repaint();
        }

        HandleShortcuts(e);

        // 아이드랍퍼: Alt+클릭 또는 I키 + 클릭
        if ((e.alt && e.type == EventType.MouseDown && e.button == 0) || (e.type == EventType.KeyDown && e.keyCode == KeyCode.I))
        {
            if (TryPickPrefabFromScene(out var picked)) { SelectPrefab(picked); e.Use(); return; }
        }

        // 마우스 위치 → 평면 교차
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!RayToPlane(ray, out var hitWorld)) return;

        var uv = _plane.WorldToUV(hitWorld);
        var snappedUV = _plane.SnapUV(uv);
        var snappedWorld = _plane.UVToWorld(snappedUV);

        // 미리보기
        Handles.color = _eraseMode ? Color.red : Color.cyan;
        Handles.DrawWireDisc(snappedWorld, _plane.Normal, 0.25f);

        bool shift = e.shift;
        bool ctrl = e.control || e.command;

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            if (shift)
            {
                if (!_lineModeActive) { _lineModeActive = true; _lineStartUV = snappedUV; }
                else { PaintLine(_lineStartUV.Value, snappedUV, _eraseMode || ctrl); _lineModeActive = false; _lineStartUV = null; }
                e.Use();
            }
            else
            {
                if (_eraseMode || ctrl) EraseAt(snappedUV);
                else PaintAt(snappedUV);
                e.Use();
            }
        }

        if (_lineModeActive && _lineStartUV.HasValue)
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(_plane.UVToWorld(_lineStartUV.Value), snappedWorld);
            sv.Repaint(); // 뷰만 리페인트
        }
    }

    void HandleShortcuts(Event e)
    {
        if (e.type == EventType.KeyDown)
        {
            
            if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha9)
            {
                int idx = (int)e.keyCode - (int)KeyCode.Alpha1;
                var p = _hotbar[idx]; if (p) { SelectPrefab(p); e.Use(); }
            }
            else if (e.keyCode == KeyCode.Q) { CyclePrefab(-1); e.Use(); }
            else if (e.keyCode == KeyCode.E) { CyclePrefab(+1); e.Use(); }
            else if (e.keyCode == KeyCode.R) { _rotationSteps = (_rotationSteps + 1) % 4; e.Use(); }
            else if (e.keyCode == KeyCode.Tab) { _eraseMode = !_eraseMode; e.Use(); }
        }
    }

    void CyclePrefab(int dir)
    {
        var favs = _palette ? _palette.items.Where(i => i.favorite && i.prefab).Select(i => i.prefab).ToList() : new List<GameObject>();
        var hb = _hotbar.Where(p => p).ToList();
        var all = _palette ? _palette.items.Where(i => i.prefab).Select(i => i.prefab).ToList() : new List<GameObject>();

        List<GameObject> ring = new List<GameObject>();
        ring.AddRange(favs);
        foreach (var p in hb) if (!ring.Contains(p)) ring.Add(p);
        foreach (var p in all) if (!ring.Contains(p)) ring.Add(p);
        if (ring.Count == 0) return;

        int cur = Mathf.Max(0, ring.IndexOf(_activePrefab));
        int nxt = (cur + dir + ring.Count) % ring.Count;
        SelectPrefab(ring[nxt]);
    }

    bool TryPickPrefabFromScene(out GameObject prefab)
    {
        prefab = null;
        var picked = HandleUtility.PickGameObject(Event.current.mousePosition, false);
        if (!picked) return false;
        var source = PrefabUtility.GetCorrespondingObjectFromSource(picked);
        if (source is GameObject go) { prefab = go; return true; }
        return false;
    }

    bool RayToPlane(Ray ray, out Vector3 hit)
    {
        Plane p = new Plane(_plane.Normal, _plane.Origin);
        if (p.Raycast(ray, out float dist)) { hit = ray.origin + ray.direction * dist; return true; }
        hit = default; return false;
    }

    void PaintAt(Vector2 centerUV)
    {
        if (!_activePrefab) return;
        int r = Mathf.RoundToInt(Mathf.Max(1, _brushSize));
        for (int y = -r + 1; y <= r - 1; y++)
        {
            for (int x = -r + 1; x <= r - 1; x++)
            {
                Vector2 uv = new Vector2(centerUV.x + x * _plane.gridSize, centerUV.y + y * _plane.gridSize);
                SpawnOnce(uv);
            }
        }
    }

    void PaintLine(Vector2 aUV, Vector2 bUV, bool erase)
    {
        Vector2 dir = (bUV - aUV);
        int steps = Mathf.CeilToInt(dir.magnitude / Mathf.Max(_plane.gridSize, 0.0001f));
        if (steps < 1) steps = 1;
        for (int i = 0; i <= steps; i++)
        {
            Vector2 uv = _plane.SnapUV(Vector2.Lerp(aUV, bUV, i / (float)steps));
            if (erase) EraseAt(uv); else SpawnOnce(uv);
        }
    }

    void SpawnOnce(Vector2 uv)
    {
        var world = _plane.UVToWorld(uv);
        float eps = _plane.gridSize * 0.2f;
        if (Physics.CheckSphere(world, eps)) return;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(_activePrefab);
        Undo.RegisterCreatedObjectUndo(go, "Level Brush Paint");
        go.tag = BRUSH_TAG;
        go.transform.position = world;
        go.transform.rotation = Quaternion.AngleAxis(90f * _rotationSteps, _plane.Normal) * go.transform.rotation;
        if (_parent) go.transform.SetParent(_parent, true);
    }

    

    void EraseAt(Vector2 uv)
    {
        var world = _plane.UVToWorld(uv);
        float r = _plane.gridSize * 0.4f;

        int count = Physics.OverlapSphereNonAlloc(world, r, _eraseBuf);
        for (int i = 0; i < count; i++)
        {
            var h = _eraseBuf[i];
            if (h.CompareTag(BRUSH_TAG))
            {
                Undo.DestroyObjectImmediate(h.transform.gameObject);
            }
        }
    }

}
