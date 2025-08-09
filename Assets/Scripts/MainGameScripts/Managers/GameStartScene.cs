using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartScene : MonoBehaviour
{
    public GameObject GameStart;
    public Button StartButton;
    public Button LoadButton;
    public Button SettingButton;
    public Button EndButton;
    public GameObject EndChoice;
    public Button YesButton;
    public Button NoButton;
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
        GameStart.SetActive(true);
        //SceneManager.LoadScene("GameStart");
    }
}
