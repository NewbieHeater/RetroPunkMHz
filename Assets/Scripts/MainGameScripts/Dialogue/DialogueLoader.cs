using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string id;           // �׷� �� ��� ��� ��ȣ ("1", "2", "3" ��)
    public string speaker;      // "NPC" or "Player" Ȥ�� Ư�� ȭ�� ID
    public string nameTag;      // ȭ�鿡 ǥ���� ȭ�� �̸� (��: "���� ���")
    public string sprite;       // ǥ��/���� ���� Ű (��: "happy", "angry")
    public string text;         // ���� ��� �ؽ�Ʈ
    public Choice[] choices;    // ������ ��� (������ null �Ǵ� �� �迭)
    public string next;         // choices�� ���� �� ����� ���� ��� ID, ������ ��ȭ ����
    public string eventKey;     // ��� ���� �� �߻��� �̺�Ʈ Ű
}

[System.Serializable]
public class Choice
{
    public string choiceText;   // ������ �ؽ�Ʈ
    public string next;         // ���� �� ������ ��� ��� ID
    public string sprite;       // ���� �� ĳ���� ǥ��
    public EventInfo[] events;  // ���� �� �߻��ϴ� �̺�Ʈ ���
}

[System.Serializable]
public class EventInfo
{
    public string type;   // �̺�Ʈ ���� ("quest", "item", "gate", etc.)
    public string action; // ���� ("accept", "complete", "give", "open", etc.)
    public string id;     // ��� ID (����ƮID, ������ID ��)
    public string target; // �ΰ� ��� (��: gate ���� ��� ������Ʈ �̸�)
    public int amount;    // ���� �� �߰� ���� (�ʿ� ��)
}

[System.Serializable]
public class DialogueGroup
{
    public string groupName;      // "Quest1", "Quest2", "Normal" ��
    public DialogueLine[] lines;  // �ش� �׷� ���� ���� (ID�� 1,2,3�� ����)
}

[System.Serializable]
public class NPCDialogueData
{
    public DialogueGroup[] groups;
}

public class DialogueLoader : MonoBehaviour
{
    // �׷���� key��, �ش� �׷�(DialogueGroup)�� value�� �ϴ� ��ųʸ�
    private Dictionary<string, DialogueGroup> groupDictionary = new Dictionary<string, DialogueGroup>();

    private NPCDialogueData loadedData;

    void Awake()
    {
        // ���ϴ� Ÿ�ֿ̹� LoadDialogueData()�� ȣ���ϰų� Awake���� �ڵ� �ε��� �� ����.
        // ��: LoadDialogueData("NPCDialogues/MyDialogueFile");
    }

    // Resources ���� ���� JSON ���� �̸�(��� ����, Ȯ���� ����)�� ���ڷ� ����
    public bool LoadDialogueData(string resourceName)
    {
        groupDictionary.Clear();

        TextAsset jsonFile = Resources.Load<TextAsset>(resourceName);
        if (jsonFile == null)
        {
            Debug.LogError("Resources ������ JSON ������ �����ϴ�: " + resourceName);
            return false;
        }
        Debug.Log("JSON ���� ����: " + jsonFile.text);

        loadedData = JsonUtility.FromJson<NPCDialogueData>(jsonFile.text);
        if (loadedData == null || loadedData.groups == null)
        {
            Debug.LogError("JSON �Ľ� ����! NPCDialogueData �Ǵ� groups�� null�Դϴ�.");
            return false;
        }

        // �� �׷��� �׷��(key)���� ��ųʸ��� ����
        foreach (var group in loadedData.groups)
        {
            if (!groupDictionary.ContainsKey(group.groupName))
            {
                groupDictionary.Add(group.groupName, group);
            }
            else
            {
                Debug.LogWarning("�ߺ��� �׷�� �߰�: " + group.groupName);
            }
        }
        return true;
    }

    // Ư�� �׷�(��: "Quest1", "Quest2", "Normal")�� ��ȭ �����͸� ��ȯ
    public DialogueGroup GetDialogueGroup(string groupName)
    {
        DialogueGroup group;
        groupDictionary.TryGetValue(groupName, out group);
        return group;
    }
}
