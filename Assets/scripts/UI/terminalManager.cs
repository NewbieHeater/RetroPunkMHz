using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class terminalManager : MonoBehaviour
{
    public GameObject dirLine;
    public GameObject responseLine;

    public TMP_InputField terminalInput;
    public GameObject userInputLine;
    public ScrollRect scrollRect;
    public GameObject msgList;

    Interpreter interpreter;

    private void Start()
    {
        interpreter = GetComponent<Interpreter>();
    }

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

            // ������ �̸� ����
            GameObject response = Instantiate(responseLine, msgList.transform);
            response.transform.SetAsLastSibling();

            // ���� ����
            Vector2 listSize = msgList.GetComponent<RectTransform>().sizeDelta;
            msgList.GetComponent<RectTransform>().sizeDelta = new Vector2(listSize.x, listSize.y + 35f);

            //�Է� ������ �������ٷ� �ű�� ����ϴµ��� �����
            userInputLine.transform.SetAsLastSibling();
            userInputLine.SetActive(false);

            Text uiText = response.GetComponentInChildren<Text>();

            StartCoroutine(interpreter.Interpret(userInput,uiText, () =>
            {
                userInputLine.SetActive(true);

                //�Է� ������ �������ٷ� �ű��
                userInputLine.transform.SetAsLastSibling();
                
                //�ٴ������� ��ũ��
                scrollRect.verticalNormalizedPosition = 0f;

                //�Է� �ʵ� ����Ŀ��
                terminalInput.ActivateInputField();
                terminalInput.Select();
            }));
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

    int AddInterpreterLines(List<string> interpretation)
    {
        for(int i = 0; i < interpretation.Count; i++)
        {
            //�亯 ��üȭ
            GameObject response = Instantiate(responseLine, msgList.transform);

            //�������� ��ġ�ϵ��� ��
            response.transform.SetAsLastSibling();

            //�޽��� ����Ʈ�� ũ�⸦ �޾ƿ� �� ��������
            Vector2 listSize = msgList.GetComponent<RectTransform>().sizeDelta;
            msgList.GetComponent<RectTransform>().sizeDelta = new Vector2(listSize.x, listSize.y + 35f);

            response.GetComponentInChildren<Text>().text = interpretation[i];

        }

        return interpretation.Count;
    }

    void ScrollToBottom(int lines)
    {
        if(lines > 4)
        {
            scrollRect.velocity = new Vector2(0, 900);
        }
        else
        {
            scrollRect.verticalNormalizedPosition = 0;
        }
    }

}
