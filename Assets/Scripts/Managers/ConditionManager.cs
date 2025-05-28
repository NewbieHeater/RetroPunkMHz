using System.Collections.Generic;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    public static ConditionManager Instance { get; private set; }
    //struct condition
    //{
    //    string conditionName;
    //    bool conditionValue;
    //}
    //// 조건 이름과 해당 조건의 달성 여부를 저장하는 사전
    //private Dictionary<string, condition> conditions = new Dictionary<string, condition>();

    // 조건 이름과 해당 조건의 달성 여부를 저장하는 사전
    private Dictionary<string, bool> conditions = new Dictionary<string, bool>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // 조건을 설정하는 메서드
    public void SetCondition(string conditionName, bool value)
    {
        if (conditions.ContainsKey(conditionName))
        {
            conditions[conditionName] = value;
        }
        else
        {
            conditions.Add(conditionName, value);
        }
    }

    // 조건의 달성 여부를 확인하는 메서드
    public bool CheckCondition(string conditionName)
    {
        if (conditions.ContainsKey(conditionName))
            return conditions[conditionName];
        return false;
    }
}
