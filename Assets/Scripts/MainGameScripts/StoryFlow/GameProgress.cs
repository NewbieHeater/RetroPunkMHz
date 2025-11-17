using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameProgress : MonoBehaviour
{
    public static GameProgress I { get; private set; }

    [Header("Runtime Story")]
    public StoryStream currentStream;
    public int currentNodeIndex = 0;   // ★ index가 곧 노드 id

    // 플래그, 제거된 엔티티, 태스크 상태
    private readonly HashSet<string> _flags = new();
    private readonly HashSet<string> _removedEntities = new();
    private readonly Dictionary<string, HashSet<string>> _completedTasksByNode = new();

    public event Action FlagsChanged;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---- Flags ----
    public bool HasFlag(string key) => _flags.Contains(key);

    public void SetFlag(string key, bool value = true)
    {
        if (value)
        {
            if (_flags.Add(key)) FlagsChanged?.Invoke();
        }
        else
        {
            if (_flags.Remove(key)) FlagsChanged?.Invoke();
        }
    }

    // ---- Tasks ----
    // nodeKey 예: $"{stream.name}:{index}"
    public bool IsTaskCompleted(string nodeKey, string taskId)
    {
        return _completedTasksByNode.TryGetValue(nodeKey, out var set) && set.Contains(taskId);
    }

    public void MarkTaskCompleted(string nodeKey, string taskId)
    {
        if (!_completedTasksByNode.TryGetValue(nodeKey, out var set))
        {
            set = new HashSet<string>();
            _completedTasksByNode[nodeKey] = set;
        }
        set.Add(taskId);
    }

    // ---- Entities ----
    public bool IsEntityRemoved(string entityGuid) => _removedEntities.Contains(entityGuid);

    public void RemoveEntity(string entityGuid)
    {
        if (_removedEntities.Add(entityGuid))
        {
            // 현재 씬에 있다면 즉시 제거
            var all = GameObject.FindObjectsOfType<SceneEntity>(true);
            foreach (var e in all)
                if (e.Guid == entityGuid)
                    GameObject.Destroy(e.gameObject);
        }
    }
}
