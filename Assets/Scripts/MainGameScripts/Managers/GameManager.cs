using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    void Awake()
    {
        //GameObject.FindWithTag("Player").TryGetComponent<PlayerManagement>(out player);
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
        }

        Paused = false;
    }
    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    public RigidPlayerManagement player;



    private static bool paused = false;
    public static bool Paused
    {
        get => paused;
        set
        {
            paused = value;
            Time.timeScale = value ? 0 : 1;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        paused = false;
        Time.timeScale = 1;
    }

    public void GamePause()
    {
        Paused = !Paused;
    }

    private void FindPlayer()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null)
            player = go.GetComponent<RigidPlayerManagement>();
        else
            player = null;
    }
}
