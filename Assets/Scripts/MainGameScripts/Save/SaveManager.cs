using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    string path;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        path = Application.persistentDataPath + "/save.json";
    }

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log("SAVE FILE PATH: " + path);
    }

    public SaveData Load()
    {
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }
}
