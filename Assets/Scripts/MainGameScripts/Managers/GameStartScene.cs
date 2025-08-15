using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartScene : MonoBehaviour
{
    [SerializeField] private GameObject _gameStart;
    [SerializeField] private GameObject _endChoice;
    [SerializeField] private GameObject _settingScreen;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _endButton;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private void Start()
    {
        _startButton.onClick.AddListener(StartNewGame);
        _loadButton.onClick.AddListener(LoadGame);
        _settingButton.onClick.AddListener(OpenSettings);
        _endButton.onClick.AddListener(QuitGame);
        _endChoice.SetActive(false);
        _yesButton.onClick.AddListener(EndGame);
        _noButton.onClick.AddListener(ReturnGame);
        _settingScreen.SetActive(false);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(_settingScreen.activeSelf)
            {
                _settingScreen.SetActive(false);
            }

        }
    }
    private void StartNewGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void LoadGame()
    {
        Debug.Log("불러오기");
        //SceneManager.LoadScene("GameScene");
    }

    private void OpenSettings()
    {
        _settingScreen.SetActive(true);
    }

    private void QuitGame()
    {
        _gameStart.SetActive(false);
        _endChoice.SetActive(true);
    }

    private void EndGame()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }

    private void ReturnGame()
    {
        _endChoice.SetActive(false);
        _gameStart.SetActive(true);
        //SceneManager.LoadScene("GameStart");
    }
}
