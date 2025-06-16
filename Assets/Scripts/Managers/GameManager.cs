using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    void Awake()
    {
        if (null == instance)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        GameObject.FindWithTag("Player").TryGetComponent<PlayerManagement>(out player);
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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
