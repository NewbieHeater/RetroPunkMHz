using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameButton : MonoBehaviour
{
    private Button _button;
    public Define.Scene selectedScene;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null) _button.onClick.AddListener(GotoMinigameScene);
    }

    private void GotoMinigameScene()
    {
        Managers.Scene.LoadScene(selectedScene);
    }
}
