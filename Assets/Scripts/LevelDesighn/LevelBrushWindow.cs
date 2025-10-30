using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

// Note : 이 코드는 공부중인 코드로 최적화, 객체지향, 코딩스타일 어디하나 잘된 부분이 없음을 알림
// 아마 오류가 나서 이 코드를 찾아오게 되었을텐데
// 오류나 질문은 갠디로 연락 바람

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
    private bool _eraseMode;

    // 스크롤
    private Vector2 _mainScroll; // 전체 창 스크롤
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

    // ==== 선택/복사용 상태 ====
    private bool _selectMode; // S로 토글 (선택 모드)
    private bool _selecting; // 드래그로 선택 사각형 지정 중
    private Vector2 _selStartUV;
    private Vector2 _selEndUV;
    private List<(GameObject prefab, Vector2 uv, Quaternion rot)> _clipboard = new(); // Ctrl+C로 채움

    // 상수
    private const string _planeRefKey = "LevelBrush.LastPlaneGOID";
    private const string BRUSH_TAG = "LevelBrush";
    private static readonly Collider[] _eraseBuf = new Collider[128];

    [MenuItem("Tools/Level Brush")]
    public static void ShowWindow() => GetWindow<LevelBrushWindow>("Level Brush");

    void OnEnable()
    {
        //minSize = new Vector2(420, 300);
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
            _tagChipStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                clipping = TextClipping.Overflow,
                wordWrap = false,
            };
            _tagChipStyle.fixedHeight = 25f;

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
        if (_plane == null)
        {
            EditorPrefs.DeleteKey(_planeRefKey);
            return;
        }
        var goid = GlobalObjectId.GetGlobalObjectIdSlow(_plane);
        EditorPrefs.SetString(_planeRefKey, goid.ToString());
#endif
    }

    void OnGUI()
    {
        InitStyles();
        EditorGUIUtility.labelWidth = Mathf.Clamp(position.width * 0.35f, 90f, 220f);
        _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll, false, true);
        float vw = Mathf.Max(0f, position.width - 18f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.MaxWidth(vw));
        EditorGUI.BeginChangeCheck();
        _plane = (LevelPlane)EditorGUILayout.ObjectField("Level Plane", _plane, typeof(LevelPlane), true);
        if (EditorGUI.EndChangeCheck()) SavePlaneRef();

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

            // 상단 상태바: 활성/비활성 토글 + 단축키 안내
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                _brushEnabled = GUILayout.Toggle(
                    _brushEnabled,
                    _brushEnabled ? "브러시 활성(B)" : "브러시 비활성(B)",
                    "Button",
                    GUILayout.Height(25),
                    GUILayout.Width(150)
                );
                var wrap = new GUIStyle(EditorStyles.label) { wordWrap = true, clipping = TextClipping.Clip };
                GUILayout.Label("단축키: B 토글, 1~9 핫바, Q/E 순환, I 아이드랍퍼, Alt+클릭 픽업, Shift+클릭 직선 색칠, S 선택, Del/Ctrl+C/Ctrl+V", wrap, GUILayout.ExpandWidth(true));
            }

            _palette = (PrefabPalette)EditorGUILayout.ObjectField("Prefab Palette", _palette, typeof(PrefabPalette), false);
            _parent = (Transform)EditorGUILayout.ObjectField("Parent (optional)", _parent, typeof(Transform), true);

            EditorGUILayout.Space(4);
            _brushSize = EditorGUILayout.IntSlider("Brush Size (cells)", _brushSize, 1, 7);
            EditorGUILayout.Space(4);

            // 액션 바
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("회전 R", GUILayout.Width(70))) _rotationSteps = (_rotationSteps + 1) % 4;
                if (GUILayout.Button(_eraseMode ? "브러시 모드(E)" : "지우개 토글(E)", GUILayout.Width(120))) _eraseMode = !_eraseMode;
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
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();  // 남는 공간은 버림
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    Vector2 _tagScroll;

    void DrawSearchAndTags()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.SetNextControlName("searchField");
            _search = EditorGUILayout.TextField("검색", _search);
            if (GUILayout.Button("지우기", GUILayout.Width(60))) _search = string.Empty;
        }

        var tags = new HashSet<string> { "All" };
        if (_palette?.items != null)
            foreach (var it in _palette.items)
                if (!string.IsNullOrEmpty(it.tag)) tags.Add(it.tag);

        // ▼ 태그 전용 가로 스크롤 격리
        //_tagScroll = EditorGUILayout.BeginScrollView(_tagScroll, true, false, GUILayout.Height(48));
        using (new EditorGUILayout.HorizontalScope())
        {
            foreach (var t in tags)
            {
                bool on = (_tagFilter == t);
                if (GUILayout.Toggle(on, new GUIContent(t), _tagChipStyle,
                                     GUILayout.Height(32), GUILayout.ExpandWidth(false)) != on)
                    _tagFilter = t;

                GUILayout.Space(4);
            }
        }
        //EditorGUILayout.EndScrollView();
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

                bool isActive = (_activePrefab == p);

                if (GUI.Button(rect, content))
                {
                    if (p) SelectPrefab(p);
                }


                // 드래그 할당
                if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) && rect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is GameObject go)
                            {
                                _hotbar[i] = go;
                                Repaint();
                                break;
                            }
                        }
                    }
                    Event.current.Use();
                }

                // 번호 라벨
                var lr = rect;
                lr.x += 4;
                lr.y += 4;
                lr.width = 20;
                lr.height = 16;
                GUI.Label(lr, label, EditorStyles.boldLabel);
            }
        }
    }

    void DrawPaletteGrid()
    {
        if (!_palette)
        {
            EditorGUILayout.HelpBox("Prefab Palette을 지정하세요.", MessageType.Info);
            return;
        }

        var list = FilteredItems();
        int col = Mathf.Max(3, (int)(position.width / 90f));

        _paletteScroll = EditorGUILayout.BeginScrollView(_paletteScroll, GUILayout.Height(200));

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
                    bool isActive = (_activePrefab == prefab);

                    if (GUILayout.Button(icon, GUILayout.Width(70), GUILayout.Height(56))) SelectPrefab(prefab);

                    var clipMini = new GUIStyle(EditorStyles.miniLabel) { clipping = TextClipping.Clip };
                    EditorGUILayout.LabelField(
                        new GUIContent(string.IsNullOrEmpty(it.label) ? prefab.name : it.label, prefab.name),
                        clipMini, GUILayout.Width(72));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        it.favorite = GUILayout.Toggle(it.favorite, "★", "Button", GUILayout.Width(24));

                        if (EditorGUILayout.DropdownButton(new GUIContent("↦", "핫바로"), FocusType.Passive, GUILayout.Width(24)))
                        {
                            var menu = new GenericMenu();
                            for (int slot = 0; slot < HotbarSize; slot++)
                            {
                                int ix = slot;
                                menu.AddItem(new GUIContent($"핫바 {slot + 1}"), false, () =>
                                {
                                    _hotbar[ix] = prefab;
                                    Repaint();
                                });
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
                bool isActive = (_activePrefab == p);

                if (GUILayout.Button(new GUIContent(tex, p.name), GUILayout.Width(42), GUILayout.Height(42)))
                    SelectPrefab(p);

            }
        }
    }

    List<PrefabPalette.Entry> FilteredItems()
    {
        if (_palette == null || _palette.items == null) return new List<PrefabPalette.Entry>();

        var items = _palette.items.Where(e => e.prefab).ToList();

        if (!string.IsNullOrEmpty(_search))
            items = items.Where(e => (e.label ?? "").ToLower().Contains(_search.ToLower()) || e.prefab.name.ToLower().Contains(_search.ToLower())).ToList();

        if (_tagFilter != "All")
            items = items.Where(e => e.tag == _tagFilter).ToList();

        // 즐겨찾기 > 이름순 정렬
        items = items.OrderByDescending(e => e.favorite).ThenBy(e => string.IsNullOrEmpty(e.label) ? e.prefab.name : e.label).ToList();

        return items;
    }

    void SelectPrefab(GameObject p)
    {
        _activePrefab = p;

        // 최근 사용 목록 업데이트
        _recent.Remove(p);
        _recent.Insert(0, p);
        if (_recent.Count > RecentMax)
            _recent.RemoveAt(_recent.Count - 1);

        Repaint(); // 윈도우 UI 갱신
    }

    // ====== Scene GUI ======

    void OnSceneGUI(SceneView sv)
    {
        Event e = Event.current;

        // B: 브러시 활성/비활성 토글
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B)
        {
            _brushEnabled = !_brushEnabled;
            e.Use();
            sv.Repaint();
        }

        if (!_brushEnabled) return;
        if (_plane == null) return;

        // 마우스 이동 이벤트를 Scene View에서 강제로 받도록 설정
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
            if (TryPickPrefabFromScene(out var picked))
            {
                SelectPrefab(picked);
                e.Use();
                return;
            }
        }

        // 마우스 위치 → 평면 교차
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!RayToPlane(ray, out var hitWorld)) return;

        var uv = _plane.WorldToUV(hitWorld);
        var snappedUV = _plane.SnapUV(uv);
        var snappedWorld = _plane.UVToWorld(snappedUV);

        // 미리보기
        Handles.color = _selectMode ? Color.yellow : _eraseMode ? Color.red : Color.cyan;
        Handles.DrawWireDisc(snappedWorld, _plane.Normal, 0.25f);
        //Handles.DrawSolidDisc(snappedWorld, _plane.Normal, _plane.gridSize * 0.4f * Mathf.Max(1, _brushSize));

        bool shift = e.shift;
        bool ctrl = e.control || e.command;

        // --- 선택 모드일 때: 드래그로 사각형 지정 ---
        if (_selectMode)
        {
            // MouseDown: 선택 시작
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                _selecting = true;
                _selStartUV = snappedUV;
                _selEndUV = snappedUV;
                e.Use();
            }
            // Drag: 사각형 업데이트
            else if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && _selecting)
            {
                _selEndUV = snappedUV;
                e.Use();
            }
            // MouseUp: 선택 종료(사각형만 확정; 복사는 Ctrl+C로 별도 수행)
            else if (e.type == EventType.MouseUp && e.button == 0 && !e.alt && _selecting)
            {
                _selecting = false;
                e.Use();
            }

            // 선택 사각형 프리뷰
            if (_selecting || (_selStartUV != Vector2.zero || _selEndUV != Vector2.zero))
            {
                DrawSelectionRect(_selStartUV, _selEndUV);
            }

            // 선택 모드에서는 브러시 페인트/지우기는 잠시 비활성
            return;
        }

        // --- 일반 브러시 모드 ---
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            if (shift)
            {
                // Shift+클릭: 라인 페인트
                if (!_lineModeActive)
                {
                    _lineModeActive = true;
                    _lineStartUV = snappedUV;
                }
                else
                {
                    PaintLine(_lineStartUV.Value, snappedUV, _eraseMode || ctrl);
                    _lineModeActive = false;
                    _lineStartUV = null;
                }
                e.Use();
            }
            else
            {
                // 일반 클릭: 페인트 또는 지우기
                if (_eraseMode || ctrl) EraseAt(snappedUV);
                else PaintAt(snappedUV);
                e.Use();
            }
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt && !_lineModeActive)
        {
            // 드래그: 연속 페인트 또는 지우기
            if (_eraseMode || ctrl) EraseAt(snappedUV);
            else PaintAt(snappedUV);
            e.Use();
        }

        // 라인 모드 미리보기
        if (_lineModeActive && _lineStartUV.HasValue)
        {
            Handles.color = Color.yellow;
            Handles.DrawLine(_plane.UVToWorld(_lineStartUV.Value), snappedWorld);
            sv.Repaint(); // 뷰만 리페인트
        }
    }

    Rect GetUVRect(Vector2 a, Vector2 b)
    {
        var min = Vector2.Min(_plane.SnapUV(a), _plane.SnapUV(b));
        var max = Vector2.Max(_plane.SnapUV(a), _plane.SnapUV(b));
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    void DrawSelectionRect(Vector2 aUV, Vector2 bUV)
    {
        Rect r = GetUVRect(aUV, bUV);

        // UV 사각형을 월드 좌표계 사각형으로 변환
        Vector3 p0 = _plane.UVToWorld(new Vector2(r.xMin, r.yMin));
        Vector3 p1 = _plane.UVToWorld(new Vector2(r.xMax, r.yMin));
        Vector3 p2 = _plane.UVToWorld(new Vector2(r.xMax, r.yMax));
        Vector3 p3 = _plane.UVToWorld(new Vector2(r.xMin, r.yMax));

        // 핸들로 사각형 그리기
        Handles.color = new Color(1f, 0.85f, 0.2f, 1f);
        Handles.DrawLine(p0, p1);
        Handles.DrawLine(p1, p2);
        Handles.DrawLine(p2, p3);
        Handles.DrawLine(p3, p0);
    }

    void CacheSelection(Vector2 aUV, Vector2 bUV)
    {
        _clipboard.Clear();
        if (_plane == null) return;

        Rect r = GetUVRect(aUV, bUV);

        // 부모 범위 안에서만 복사 (원하면 옵션화)
        // BRUSH_TAG가 붙은 오브젝트만 검색
        IEnumerable<Transform> pool = GameObject.FindObjectsOfType<Transform>(true).Where(t => t.CompareTag(BRUSH_TAG));

        foreach (var t in pool)
        {
            if (!t) continue;
            // UV 좌표 계산
            _plane.ProjectToPlane(t.position, out var pOnPlane);
            var uv = _plane.WorldToUV(pOnPlane);

            // 선택 사각형 범위 내에 있는지 확인 (정확도를 위해 작은 오차 허용)
            if (uv.x >= r.xMin - 1e-4f && uv.x <= r.xMax + 1e-4f && uv.y >= r.yMin - 1e-4f && uv.y <= r.yMax + 1e-4f)
            {
                var src = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
                GameObject prefab = (src as GameObject);
                if (prefab) // 프리팹 원본이 있는 경우만 복사 대상에 추가
                {
                    _clipboard.Add((prefab, _plane.SnapUV(uv), t.rotation));
                }
            }
        }

        // 정렬(좌하→우상)해두면 붙여넣기 오프셋 계산이 일정해짐
        _clipboard = _clipboard.OrderBy(x => x.uv.y).ThenBy(x => x.uv.x).ToList();

        Debug.Log($"Cached {_clipboard.Count} objects to clipboard.");
    }

    void DeleteSelection(Vector2 aUV, Vector2 bUV)
    {
        if (_plane == null) return;

        Rect r = GetUVRect(aUV, bUV);
        // BRUSH_TAG가 붙은 오브젝트만 검색
        IEnumerable<Transform> pool = GameObject.FindObjectsOfType<Transform>(true).Where(t => t.CompareTag(BRUSH_TAG));

        int count = 0;
        foreach (var t in pool)
        {
            if (!t) continue;

            // UV 좌표 계산
            _plane.ProjectToPlane(t.position, out var pOnPlane);
            var uv = _plane.WorldToUV(pOnPlane);

            // 선택 사각형 범위 내에 있는지 확인
            if (uv.x >= r.xMin - 1e-4f && uv.x <= r.xMax + 1e-4f && uv.y >= r.yMin - 1e-4f && uv.y <= r.yMax + 1e-4f)
            {
                Undo.DestroyObjectImmediate(t.gameObject);
                count++;
            }
        }
        Debug.Log($"Deleted {count} objects in selection.");

        // 삭제 후 선택 영역 초기화
        _selStartUV = _selEndUV = Vector2.zero;
        SceneView.RepaintAll();
    }

    void PasteClipboardAt(Vector2 targetUV)
    {
        if (_clipboard.Count == 0)
        {
            Debug.Log("Clipboard is empty.");
            return;
        }

        // 복사된 객체들 중 가장 좌하단 UV 계산
        Vector2 minUV = new Vector2(
            _clipboard.Min(x => x.uv.x),
            _clipboard.Min(x => x.uv.y)
        );

        // 붙여넣을 위치(targetUV)와 minUV의 차이 = 오프셋
        Vector2 delta = _plane.SnapUV(targetUV) - minUV;

        foreach (var it in _clipboard)
        {
            if (!it.prefab) continue;

            Vector2 uv = _plane.SnapUV(it.uv + delta);
            var world = _plane.UVToWorld(uv);

            // 해당 위치에 이미 다른 오브젝트가 있는지 검사 (단순화된 검사)
            if (Physics.CheckSphere(world, _plane.gridSize * 0.2f))
            {
                // 이미 있다면 스킵하거나, 기존 오브젝트를 제거하는 로직 추가 가능
                continue;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(it.prefab);
            Undo.RegisterCreatedObjectUndo(go, "Level Brush Paste");
            go.tag = BRUSH_TAG;
            go.transform.position = world;
            go.transform.rotation = it.rot;
            if (_parent) go.transform.SetParent(_parent, true);
        }

        Debug.Log($"Pasted {_clipboard.Count} objects at {targetUV}.");
    }

    void HandleShortcuts(Event e)
    {
        if (e.type == EventType.KeyDown)
        {
            // S: 선택 모드 토글
            if (e.keyCode == KeyCode.S)
            {
                _eraseMode = false;
                _selectMode = !_selectMode;
                _selecting = false;
                _lineModeActive = false; // 라인 모드도 비활성화
                e.Use();
                Repaint(); // 윈도우 UI 갱신
                SceneView.RepaintAll();
                return;
            }

            // Ctrl+C: 현재 선택 사각형 기준으로 복사
            if ((e.control || e.command) && e.keyCode == KeyCode.C)
            {
                CacheSelection(_selStartUV, _selEndUV);
                e.Use();
                return;
            }

            // Ctrl+V: 현재 마우스 위치 기준으로 붙여넣기
            if ((e.control || e.command) && e.keyCode == KeyCode.V)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (RayToPlane(ray, out var hitWorld))
                {
                    var uv = _plane.SnapUV(_plane.WorldToUV(hitWorld));
                    PasteClipboardAt(uv);
                }
                e.Use();
                return;
            }

            // Delete: 선택 영역 삭제
            if (e.keyCode == KeyCode.Delete)
            {
                DeleteSelection(_selStartUV, _selEndUV);
                e.Use();
                return;
            }

            // 1~9: 핫바 선택
            if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha9)
            {
                int idx = (int)e.keyCode - (int)KeyCode.Alpha1;
                var p = _hotbar[idx];
                if (p)
                {
                    SelectPrefab(p);
                    e.Use();
                }
            }
            else if (e.keyCode == KeyCode.Q)
            {
                CyclePrefab(-1); // 이전
                e.Use();
            }
            else if (e.keyCode == KeyCode.E)
            {
                // E: 지우개 토글 (Q/E 순환 대신 R/E 토글로 재정의)
                _selectMode = false;
                _eraseMode = !_eraseMode;
                e.Use();
                Repaint();
                SceneView.RepaintAll();
            }
            else if (e.keyCode == KeyCode.D)
            {
                // D: 드로잉 모드
                _selectMode = false;
                _eraseMode = false;
                e.Use();
                Repaint();
                SceneView.RepaintAll();
            }
            // R: 회전
            else if (e.keyCode == KeyCode.R)
            {
                _rotationSteps = (_rotationSteps + 1) % 4;
                e.Use();
            }
        }
    }

    void CyclePrefab(int dir)
    {
        var favs = _palette ? _palette.items.Where(i => i.favorite && i.prefab).Select(i => i.prefab).ToList() : new List<GameObject>();
        var hb = _hotbar.Where(p => p).ToList();
        var all = _palette ? _palette.items.Where(i => i.prefab).Select(i => i.prefab).ToList() : new List<GameObject>();

        // 순환 링 생성: 즐겨찾기 -> 핫바 -> 팔레트 전체 (중복 제거)
        List<GameObject> ring = new List<GameObject>();
        ring.AddRange(favs);
        foreach (var p in hb)
            if (!ring.Contains(p)) ring.Add(p);
        foreach (var p in all)
            if (!ring.Contains(p)) ring.Add(p);

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

        // 원본 프리팹 가져오기
        var source = PrefabUtility.GetCorrespondingObjectFromSource(picked);
        if (source is GameObject go)
        {
            prefab = go;
            return true;
        }
        return false;
    }

    bool RayToPlane(Ray ray, out Vector3 hit)
    {
        // LevelPlane의 노멀과 원점을 이용해 평면 생성
        Plane p = new Plane(_plane.Normal, _plane.Origin);
        if (p.Raycast(ray, out float dist))
        {
            hit = ray.origin + ray.direction * dist;
            return true;
        }
        hit = default;
        return false;
    }

    void PaintAt(Vector2 centerUV)
    {
        if (!_activePrefab) return;

        int r = Mathf.RoundToInt(Mathf.Max(1, _brushSize));

        // 브러시 크기에 따라 범위 지정
        for (int y = -r + 1; y <= r - 1; y++)
        {
            for (int x = -r + 1; x <= r - 1; x++)
            {
                // 격자 크기에 맞춰 UV 좌표 계산
                Vector2 uv = _plane.SnapUV(new Vector2(centerUV.x + x * _plane.gridSize, centerUV.y + y * _plane.gridSize));
                SpawnOnce(uv);
            }
        }
    }

    void PaintLine(Vector2 aUV, Vector2 bUV, bool erase)
    {
        Vector2 dir = (bUV - aUV);
        // 격자 크기를 기준으로 몇 걸음을 걸어야 하는지 계산
        int steps = Mathf.CeilToInt(dir.magnitude / Mathf.Max(_plane.gridSize, 0.0001f));
        if (steps < 1) steps = 1;

        for (int i = 0; i <= steps; i++)
        {
            Vector2 uv = _plane.SnapUV(Vector2.Lerp(aUV, bUV, i / (float)steps));
            if (erase) EraseAt(uv);
            else SpawnOnce(uv);
        }
    }

    void SpawnOnce(Vector2 uv)
    {
        var world = _plane.UVToWorld(uv);
        float eps = _plane.gridSize * 0.2f;

        // 해당 위치에 이미 오브젝트가 있는지 검사 (겹침 방지)
        if (Physics.CheckSphere(world, eps)) return;

        // 프리팹 생성
        var go = (GameObject)PrefabUtility.InstantiatePrefab(_activePrefab);
        Undo.RegisterCreatedObjectUndo(go, "Level Brush Paint");
        go.tag = BRUSH_TAG;
        go.transform.position = world;

        // 회전 적용
        go.transform.rotation = Quaternion.AngleAxis(90f * _rotationSteps, _plane.Normal) * go.transform.rotation;

        // 부모 설정
        if (_parent) go.transform.SetParent(_parent, true);
    }

    void EraseAt(Vector2 uv)
    {
        var world = _plane.UVToWorld(uv);
        // 브러시 크기에 따른 삭제 반경 설정
        float r = _plane.gridSize * 0.4f * Mathf.Max(1, _brushSize);

        // 해당 위치 주변의 콜라이더 검색
        int count = Physics.OverlapSphereNonAlloc(world, r, _eraseBuf);

        for (int i = 0; i < count; i++)
        {
            var h = _eraseBuf[i];
            // BRUSH_TAG가 붙은 오브젝트만 삭제
            if (h.CompareTag(BRUSH_TAG))
            {
                Undo.DestroyObjectImmediate(h.transform.gameObject);
            }
        }
    }
}