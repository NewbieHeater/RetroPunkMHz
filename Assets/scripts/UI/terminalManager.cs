using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class terminalManager : MonoBehaviour
{
    public GameObject dirLine;
    public GameObject responseLine;

    public InputField terminalInput;
    public GameObject userInputLine;
    public ScrollRect scrollRect;
    public GameObject msgList;

    private void OnGUI()
    {
        if(terminalInput.isFocused && terminalInput.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            //������ �Է��� ����
            string userInput = terminalInput.text;

            //��ǲ �ʵ� Ŭ����
            ClearInputField();

            //���丮 ���� �ν��Ͻ�ȭ
            AddDirectoryLine(userInput);

            //�Է� ������ �������ٷ� �ű��
            userInputLine.transform.SetAsLastSibling();

            //�Է� �ʵ� ����Ŀ��
            terminalInput.ActivateInputField();
            terminalInput.Select();
        }
    }

    void ClearInputField()
    {
        terminalInput.text = "";
    }

    void AddDirectoryLine(string userInput)
    {
        //Ŀ�ǵ� ���� �����̳� ��������
        Vector2 msgListSize = msgList.GetComponent<RectTransform>().sizeDelta;
        msgList.GetComponent<RectTransform>().sizeDelta = new Vector2(msgListSize.x, msgListSize.y + 35f);

        //���丮 ���� ��ü ����
        GameObject msg = Instantiate(dirLine, msgList.transform);

        //�ڽİ�ü �ε��� ����
        msg.transform.SetSiblingIndex(msgList.transform.childCount - 1);

        //������ ��ü �ؽ�Ʈ ����
        msg.GetComponentsInChildren<Text>()[1].text = userInput;
    }

}
