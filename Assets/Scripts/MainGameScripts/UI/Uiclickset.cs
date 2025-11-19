using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Uiclickset : MonoBehaviour
{
    private EtoRoominstall _placementManager;
    private float _lastclick;
    private float _doubleclicktime = 0.3f;

    private void Start()
    {
        
        SceneManager.activeSceneChanged += OnSceneChanged;
        TryFindPlacementManager();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        TryFindPlacementManager();
    }

    private void TryFindPlacementManager()
    {
        _placementManager = FindObjectOfType<EtoRoominstall>();

        if (_placementManager == null)
            Debug.Log(" 현재 씬에 EtoRoominstall 없음");
        else
            Debug.Log("EtoRoominstall 연결 완료: ");
    }


    public void HandleInventoryClick(GameObject itemPrefab)
    {
        Debug.Log("짜잔");
        if (_placementManager == null)
        {
            Debug.Log("설치 매니저 없음 (에토방이 아님)");
            return;
        }

        if (Time.deltaTime - _lastclick < _doubleclicktime)
        {
            Debug.Log("짜잔");
            _placementManager.startPlacing(_itemprefab);

        }
        _lastclick = Time.time;
    }
}

    
