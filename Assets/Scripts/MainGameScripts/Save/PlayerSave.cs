using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour, ISaveable
{
    public int hp = 100;

    public static PlayerSave Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveData(SaveData data)
    {
        data.playerPosition = transform.position;
        data.playerHp = hp;
    }

    public void LoadData(SaveData data)
    {
        transform.position = data.playerPosition;
        hp = data.playerHp;
    }
}
