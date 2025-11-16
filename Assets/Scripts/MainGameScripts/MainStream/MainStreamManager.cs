using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MainStreamManager : MonoBehaviour
{
    public static MainStreamManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("초기 상태 (게임 시작 시)")]
    [SerializeField] private MainStreamStateObj initialState;

    [Header("세이브/로드용 레지스트리")]
    [SerializeField] private MainStreamStateObj[] allStates;
    [SerializeField] private StoryBoolVar[] allBoolVars;
    [SerializeField] private StoryIntVar[] allIntVars;
    [SerializeField] private MainStreamConditionFlag[] allFlags;

    public MainStreamStateObj CurrentState { get; private set; }

    /// <summary>상태가 바뀔 때마다 씬 내 리액터에게 알려줄 이벤트</summary>
    public event Action<MainStreamStateObj> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnterState(initialState);
    }

    // ---------- 상태 진행 ----------

    void EnterState(MainStreamStateObj state)
    {
        CurrentState = state;

        if (CurrentState != null)
            Debug.Log($"[MainStream] Enter : {CurrentState.stateId}");
        else
            Debug.Log("[MainStream] Finished.");

        OnStateChanged?.Invoke(CurrentState);

        // 조건이 없고, autoClearIfNoCondition이면 바로 클리어
        if (CurrentState != null &&
            (CurrentState.clearConditions == null || CurrentState.clearConditions.Count == 0) &&
            CurrentState.autoClearIfNoCondition)
        {
            ClearCurrentStateAndProgress();
        }
    }

    bool AreClearConditionsMet(MainStreamStateObj state)
    {
        if (state == null) return false;

        if (state.clearConditions == null || state.clearConditions.Count == 0)
            return state.autoClearIfNoCondition;

        foreach (var cond in state.clearConditions)
        {
            if (cond == null) continue;
            if (!cond.IsMet()) return false;
        }
        return true;
    }

    MainStreamStateObj ResolveNextState(MainStreamStateObj state)
    {
        if (state == null) return null;

        if (state.branches != null)
        {
            foreach (var branch in state.branches)
            {
                if (branch == null || branch.nextState == null) continue;
                if (branch.conditions == null || branch.conditions.Count == 0)
                    continue;

                bool ok = true;
                foreach (var cond in branch.conditions)
                {
                    if (cond == null) continue;
                    if (!cond.IsMet())
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    Debug.Log($"[MainStream] Branch selected : {branch.branchName}");
                    return branch.nextState;
                }
            }
        }

        return state.defaultNextState;
    }

    void ClearCurrentStateAndProgress()
    {
        if (CurrentState == null) return;

        Debug.Log($"[MainStream] Clear : {CurrentState.stateId}");

        // 공통 클리어 이벤트 실행 (대사 출력 등)
        CurrentState.onCleared?.Invoke();

        // 분기 해석 후 다음 상태로
        var next = ResolveNextState(CurrentState);
        EnterState(next);
    }

    /// <summary>
    /// FlagCondition 등에서 사용. 플래그가 Activate 되었을 때 현재 상태 재평가.
    /// </summary>
    public void OnConditionActivated(MainStreamConditionFlag flag)
    {
        if (CurrentState == null) return;

        // 현재 상태의 clearConditions 중 FlagCondition이 있을 수 있으므로
        if (AreClearConditionsMet(CurrentState))
            ClearCurrentStateAndProgress();
    }

    public bool IsCurrent(string stateId)
        => CurrentState != null && CurrentState.stateId == stateId;

    // ---------- 세이브 / 로드 ----------

    string GetSaveKey(int slot) => $"MainStream_Save_{slot}";

    MainStreamStateObj FindStateById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var s in allStates)
        {
            if (s != null && s.stateId == id)
                return s;
        }
        return null;
    }

    public void Save(int slot)
    {
        var data = new MainStreamSaveData();

        // 현재 상태
        data.currentStateId = CurrentState != null ? CurrentState.stateId : "";

        // Bool 변수
        var boolList = new List<BoolVarEntry>();
        foreach (var v in allBoolVars)
        {
            if (v == null) continue;
            boolList.Add(new BoolVarEntry { key = v.Key, value = v.Value });
        }
        data.boolVars = boolList.ToArray();

        // Int 변수
        var intList = new List<IntVarEntry>();
        foreach (var v in allIntVars)
        {
            if (v == null) continue;
            intList.Add(new IntVarEntry { key = v.Key, value = v.Value });
        }
        data.intVars = intList.ToArray();

        // 플래그
        var flagList = new List<FlagEntry>();
        foreach (var f in allFlags)
        {
            if (f == null) continue;
            flagList.Add(new FlagEntry { key = f.ConditionName, value = f.IsActive });
        }
        data.flags = flagList.ToArray();

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(GetSaveKey(slot), json);
        PlayerPrefs.Save();

        Debug.Log($"[MainStream] Saved to slot {slot}");
    }

    public bool Load(int slot)
    {
        string key = GetSaveKey(slot);
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.LogWarning($"[MainStream] No save data in slot {slot}");
            return false;
        }

        string json = PlayerPrefs.GetString(key);
        var data = JsonUtility.FromJson<MainStreamSaveData>(json);
        if (data == null)
        {
            Debug.LogError("[MainStream] Load failed: invalid JSON");
            return false;
        }

        // 상태 복원
        var state = FindStateById(data.currentStateId);
        if (state == null)
        {
            Debug.LogWarning($"[MainStream] State '{data.currentStateId}' not found. Fallback to initial.");
            state = initialState;
        }

        // Bool 변수 복원
        if (data.boolVars != null)
        {
            foreach (var e in data.boolVars)
            {
                foreach (var v in allBoolVars)
                {
                    if (v != null && v.Key == e.key)
                    {
                        v.Value = e.value;
                        break;
                    }
                }
            }
        }

        // Int 변수 복원
        if (data.intVars != null)
        {
            foreach (var e in data.intVars)
            {
                foreach (var v in allIntVars)
                {
                    if (v != null && v.Key == e.key)
                    {
                        v.Value = e.value;
                        break;
                    }
                }
            }
        }

        // 플래그 복원 (콜백 없이)
        if (data.flags != null)
        {
            foreach (var e in data.flags)
            {
                foreach (var f in allFlags)
                {
                    if (f != null && f.ConditionName == e.key)
                    {
                        f.SetFromSave(e.value);
                        break;
                    }
                }
            }
        }

        // 로드 후 상태 진입 (NPC 등 리액터를 위해 이벤트 발생)
        EnterState(state);

        Debug.Log($"[MainStream] Loaded from slot {slot}");
        return true;
    }
}
