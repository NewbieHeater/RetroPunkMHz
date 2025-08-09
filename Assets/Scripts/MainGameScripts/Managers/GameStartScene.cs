using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartScene : MonoBehaviour
{
    [SerializeField] private GameObject GameStart;
    [SerializeField] private GameObject EndChoice;

    [SerializeField] private Button StartButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button SettingButton;
    [SerializeField] private Button EndButton;
    [SerializeField] private Button YesButton;
    [SerializeField] private Button NoButton;

    private void Start()
    {
        StartButton.onClick.AddListener(StartNewGame);
        LoadButton.onClick.AddListener(LoadGame);
        SettingButton.onClick.AddListener(OpenSettings);
        EndButton.onClick.AddListener(QuitGame);
        EndChoice.SetActive(false);
        YesButton.onClick.AddListener(EndGame);
        NoButton.onClick.AddListener(ReturnGame);
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

    }

    private void QuitGame()
    {
        GameStart.SetActive(false);
        EndChoice.SetActive(true);
    }

    private void EndGame()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }

    private void ReturnGame()
    {
        EndChoice.SetActive(false);
        SceneManager.LoadScene("GameStart");
    }
}
