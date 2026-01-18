using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSave : MonoBehaviour, ISaveable
{
    SceneLoader sceneLoader;
    public void SaveData(SaveData data)
    {
        data.sceneName = SceneManager.GetActiveScene().name;
    }

    public void LoadData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.sceneName))
            return;

        
        if (sceneLoader == null)
            sceneLoader = FindObjectOfType<SceneLoader>();

        // 그래도 없으면 그냥 중단
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader not found");
            return;
        }

        // 같은 씬이면 다시 로드 안 함
        if (SceneManager.GetActiveScene().name == data.sceneName)
            return;

        sceneLoader.LoadScene(data.sceneName);
    }
}
