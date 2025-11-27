using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    int _order = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;
    private Dictionary<string, GameObject> panelCache = new Dictionary<string, GameObject>(); // 패널 캐싱

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };
            return root;
        }
    }


    public void Init()
    {

        //LoadAndCacheAllPanels();
    }

    // Canvas 설정
    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    // UI 서브 아이템 생성
    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");
        if (parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    // Scene UI 표시
    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        go.transform.SetParent(Root.transform);

        return sceneUI;
    }

    // Popup UI 표시
    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ObjectPooler.SpawnFromPool(name, Root.transform.position);
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);

        return popup;
    }

    // Popup UI 닫기
    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    // 가장 최근에 열린 Popup UI 닫기
    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Disable(popup.gameObject);
        popup = null;
        _order--;
    }

    // 모든 Popup UI 닫기
    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    // UI 모두 초기화
    public void Clear()
    {
        CloseAllPopupUI();
        _sceneUI = null;
    }

    // 패널 로드 및 캐싱
    //public void LoadAndCacheAllPanels()
    //{
    //    // UI/Panel 경로 내의 모든 패널을 로드
    //    GameObject[] panelPrefabs = Managers.Resource.LoadPrefab("UI/Scene");

    //    if (panelPrefabs.Length > 0)
    //    {
    //        foreach (GameObject panelPrefab in panelPrefabs)
    //        {
    //            // 이미 캐시된 패널은 건너뜀
    //            if (!panelCache.ContainsKey(panelPrefab.name))
    //            {
    //                GameObject panelInstance = Object.Instantiate(panelPrefab, Root.transform);
    //                panelInstance.SetActive(false); // 기본적으로 비활성화
    //                panelCache[panelPrefab.name] = panelInstance;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("No panels found in the 'UI/Scene' path.");
    //    }
    //}


    //// 패널 전환
    //public void SwitchPanel(string panelName)
    //{
    //    // 현재 활성화된 패널이 있다면 비활성화
    //    foreach (var cachedpanel in panelCache.Values)
    //    {
    //        cachedpanel.SetActive(false);
    //    }

    //    // 해당 패널이 캐시되어 있다면 활성화
    //    if (panelCache.TryGetValue(panelName, out GameObject panel))
    //    {
    //        panel.SetActive(true);
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"Panel '{panelName}' not loaded.");
    //    }
    //}
}