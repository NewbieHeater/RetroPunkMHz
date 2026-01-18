using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    ISaveable[] saveables;
    SaveData pendingLoadData;

    private void Awake()
    {
        CollectSaveables();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void CollectSaveables()
    {
        saveables = FindObjectsOfType<MonoBehaviour>()
                    .OfType<ISaveable>()
                    .ToArray();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SaveGame();

        if (Input.GetKeyDown(KeyCode.F9))
            LoadGame();
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

        pendingLoadData = data;

        string currentScene = SceneManager.GetActiveScene().name;

        // 씬이 다르면 → 씬 로드 후 나머지
        if (currentScene != data.sceneName)
        {
            SceneSave sceneSave = FindObjectOfType<SceneSave>();
            if (sceneSave == null)
            {
                Debug.LogError("SceneSave not found");
                return;
            }

            sceneSave.LoadData(data); // 씬 이동
        }
        //씬이 같으면 → 바로 나머지 로드
        else
        {
            ApplyLoadedData();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadData == null) return;
        ApplyLoadedData();
    }
    void ApplyLoadedData()
    {
        CollectSaveables();

        foreach (var s in saveables)
        {
            if (s is SceneSave) continue;
            s.LoadData(pendingLoadData);
        }

        pendingLoadData = null;
        Debug.Log("LOAD COMPLETE");
    }
}