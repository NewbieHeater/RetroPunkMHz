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

    public PlayerManagement player;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
    }
    private void FindPlayer()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null)
            player = go.GetComponent<PlayerManagement>();
        else
            player = null;
    }
}
