using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class UIMove : MonoBehaviour
{
    private Button[] _buttons;
    private int _currentIndex = 0;

    void Start()
    {
        _buttons = GetComponentsInChildren<Button>();

        if (_buttons.Length > 0&& (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)))
        {
           
            EventSystem.current.SetSelectedGameObject(_buttons[_currentIndex].gameObject);
        }
        
    }

    void Update()
    {
        if (_buttons.Length == 0 || EventSystem.current == null) return;

        if (Input.GetKeyDown(KeyCode.W)) // 위로
        {
            
            _currentIndex = (_currentIndex - 1 + _buttons.Length) % _buttons.Length;
            EventSystem.current.SetSelectedGameObject(_buttons[_currentIndex].gameObject);
        }
        if (Input.GetKeyDown(KeyCode.S)) // 아래로
        {
            _currentIndex = (_currentIndex + 1) % _buttons.Length;
            EventSystem.current.SetSelectedGameObject(_buttons[_currentIndex].gameObject);
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i].gameObject == selected)
                {
                    _currentIndex = i;
                    break;
                }
            }
        }
    }
}
