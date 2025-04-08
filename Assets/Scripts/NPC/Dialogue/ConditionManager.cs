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
    //// ���� �̸��� �ش� ������ �޼� ���θ� �����ϴ� ����
    //private Dictionary<string, condition> conditions = new Dictionary<string, condition>();

    // ���� �̸��� �ش� ������ �޼� ���θ� �����ϴ� ����
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

    // ������ �����ϴ� �޼���
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

    // ������ �޼� ���θ� Ȯ���ϴ� �޼���
    public bool CheckCondition(string conditionName)
    {
        if (conditions.ContainsKey(conditionName))
            return conditions[conditionName];
        return false;
    }
}
