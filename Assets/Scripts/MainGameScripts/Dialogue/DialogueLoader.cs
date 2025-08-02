using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string id;           // 그룹 내 대사 노드 번호 ("1", "2", "3" 등)
    public string speaker;      // "NPC" or "Player" 혹은 특정 화자 ID
    public string nameTag;      // 화면에 표시할 화자 이름 (예: "성문 경비병")
    public string sprite;       // 표정/감정 상태 키 (예: "happy", "angry")
    public string text;         // 실제 대사 텍스트
    public Choice[] choices;    // 선택지 목록 (없으면 null 또는 빈 배열)
    public string next;         // choices가 없을 때 사용할 다음 대사 ID, 없으면 대화 종료
    public string eventKey;     // 대사 진행 중 발생할 이벤트 키
}

[System.Serializable]
public class Choice
{
    public string choiceText;   // 선택지 텍스트
    public string next;         // 선택 후 진행할 대사 노드 ID
    public string sprite;       // 선택 후 캐릭터 표정
    public EventInfo[] events;  // 선택 시 발생하는 이벤트 목록
}

[System.Serializable]
public class EventInfo
{
    public string type;   // 이벤트 종류 ("quest", "item", "gate", etc.)
    public string action; // 동작 ("accept", "complete", "give", "open", etc.)
    public string id;     // 대상 ID (퀘스트ID, 아이템ID 등)
    public string target; // 부가 대상 (예: gate 열기 대상 오브젝트 이름)
    public int amount;    // 수량 등 추가 정보 (필요 시)
}

[System.Serializable]
public class DialogueGroup
{
    public string groupName;      // "Quest1", "Quest2", "Normal" 등
    public DialogueLine[] lines;  // 해당 그룹 내의 대사들 (ID는 1,2,3… 형식)
}

[System.Serializable]
public class NPCDialogueData
{
    public DialogueGroup[] groups;
}

public class DialogueLoader : MonoBehaviour
{
    // 그룹명을 key로, 해당 그룹(DialogueGroup)을 value로 하는 딕셔너리
    private Dictionary<string, DialogueGroup> groupDictionary = new Dictionary<string, DialogueGroup>();

    private NPCDialogueData loadedData;

    void Awake()
    {
        // 원하는 타이밍에 LoadDialogueData()를 호출하거나 Awake에서 자동 로드할 수 있음.
        // 예: LoadDialogueData("NPCDialogues/MyDialogueFile");
    }

    // Resources 폴더 내의 JSON 파일 이름(경로 포함, 확장자 제외)을 인자로 전달
    public bool LoadDialogueData(string resourceName)
    {
        groupDictionary.Clear();

        TextAsset jsonFile = Resources.Load<TextAsset>(resourceName);
        if (jsonFile == null)
        {
            Debug.LogError("Resources 폴더에 JSON 파일이 없습니다: " + resourceName);
            return false;
        }
        Debug.Log("JSON 파일 내용: " + jsonFile.text);

        loadedData = JsonUtility.FromJson<NPCDialogueData>(jsonFile.text);
        if (loadedData == null || loadedData.groups == null)
        {
            Debug.LogError("JSON 파싱 실패! NPCDialogueData 또는 groups가 null입니다.");
            return false;
        }

        // 각 그룹을 그룹명(key)으로 딕셔너리에 저장
        foreach (var group in loadedData.groups)
        {
            if (!groupDictionary.ContainsKey(group.groupName))
            {
                groupDictionary.Add(group.groupName, group);
            }
            else
            {
                Debug.LogWarning("중복된 그룹명 발견: " + group.groupName);
            }
        }
        return true;
    }

    // 특정 그룹(예: "Quest1", "Quest2", "Normal")의 대화 데이터를 반환
    public DialogueGroup GetDialogueGroup(string groupName)
    {
        DialogueGroup group;
        groupDictionary.TryGetValue(groupName, out group);
        return group;
    }
}
