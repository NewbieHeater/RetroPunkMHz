using UnityEngine;
using System.Linq;

public class GameSaveManager : MonoBehaviour
{
    ISaveable[] saveables;

    private void Awake()
    {
        saveables = FindObjectsOfType<MonoBehaviour>()
                    .OfType<ISaveable>()
                    .ToArray();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("SAVE KEY PRESSED");
            SaveGame();
        }
            

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("LOAD KEY PRESSED");
            LoadGame();
        }
           
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        foreach (var s in saveables)
            s.SaveData(data);

        SaveManager.Instance.Save(data);
        Debug.Log("SAVE COMPLETE");
    }

    public void LoadGame()
    {
        SaveData data = SaveManager.Instance.Load();
        if (data == null)
        {
            Debug.Log("NO SAVE DATA");
            return;
        }
        foreach (var s in saveables)
            s.LoadData(data);
        Debug.Log("LOAD COMPLETE");
    }
}
