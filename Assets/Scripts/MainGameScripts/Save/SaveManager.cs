using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    string path;
    private SaveData pendingData;
    private bool hasPendingLoad = false;
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
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
    public void SaveGame(RigidPlayerManagement player)
    {
        SaveData data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;
        data.playerPosition = player.transform.position;

        Save(data);
    }

    public void LoadGame()
    {
        pendingData = Load();
        if (pendingData == null)
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == pendingData.sceneName)
        {
            ApplyToCurrentScene();
        }
        else
        {
            hasPendingLoad = true;
            SceneManager.LoadScene(pendingData.sceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingLoad) return;

        ApplyToCurrentScene();

        hasPendingLoad = false;
        pendingData = null;
    }

    private void ApplyToCurrentScene()
    {
        var players = FindObjectsOfType<RigidPlayerManagement>();

        foreach (var p in players)
        {
            if (p.gameObject.scene == SceneManager.GetActiveScene())
            {
                ApplyPlayerData(p); 
                break;
            }
        }
    }
    private void ApplyPlayerData(RigidPlayerManagement player)
    {
        StartCoroutine(ApplyAfterGroundReady(player));
    }

    private IEnumerator ApplyAfterGroundReady(RigidPlayerManagement player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        GroundDetector ground = player.GetComponent<GroundDetector>();

        var move = player.GetComponent<RigidMovementController>();
        var jump = player.GetComponent<RigidJumpController>();

        // 1️모든 컨트롤러 완전 차단
        player.SetAblePlayer(false);
        if (move) move.enabled = false;
        if (jump) jump.enabled = false;

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        // 2️위치 적용
        player.transform.position = pendingData.playerPosition;

        // 3️물리 월드 안정화 대기 (여기 중요)
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // 4️바닥 인식될 때까지 대기
        if (ground != null)
        {
            int safety = 10;
            while (!ground.IsGrounded && safety-- > 0)
            {
                ground.UpdateGroundStatus();
                yield return new WaitForFixedUpdate();
            }
        }

        // 5️이제서야 물리 + 컨트롤러 재개
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;

        if (jump) jump.enabled = true;
        if (move) move.enabled = true;
        player.SetAblePlayer(true);
    }
}
