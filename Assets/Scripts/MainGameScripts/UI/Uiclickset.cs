using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Uiclickset : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject _gameObject;
    [SerializeField] private EtoRoominstall _placementManager;
    private float _lastclick;
    private float _doubleclicktime = 0.3f;




    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.deltaTime - _lastclick < _doubleclicktime)
        {
            _placementManager.startPlacing(_gameObject);

        }
        _lastclick = Time.time;
    }

}

    
