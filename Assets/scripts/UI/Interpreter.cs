using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Interpreter : MonoBehaviour
{
    RAGHandler ragHandler;
    string answer;
    private void Start()
    {
        ragHandler = GetComponent<RAGHandler>();
    }

    Dictionary<string, string> colors = new Dictionary<string, string>()
    {
        {"red",     "#ff0000"},
        {"green",   "#00ff00"},
        {"blue",    "#0000ff"},
        {"yellow",  "#ffff00"},
        {"cyan",    "#00ffff"},
        {"magenta", "#ff00ff"}
    };

    List<string> response = new List<string>();

    public IEnumerator Interpret(string userInput, Text outputTarget, System.Action onComplete)
    {
        response.Clear();

        string[] args = userInput.Split();

        if (args[0] == "/help")
        {
            response.Add("�𸣴� ���� ������ �͹̳ο��� �������.");
            response.Add("Ŀ�ǵ带 ����Ϸ��� \'/\'�ڿ� ��ɾ �Է��ϼ���");
            foreach (string i in response)
            {
                outputTarget.text += i;
            }
            yield break;
        }

        else if (args[0] == "ascii")
        {
            LoadTitle("ascii.txt", "cyan", 2);
            foreach (string i in response)
            {
                outputTarget.text += i;
            }
            yield break;
        }

        else if (args[0] == "/selfDestroy")
        {
            response.Add("���� ������ ����");
            ListEntry("3", "���� ����...");
            ListEntry("2", "���� ����...");
            ListEntry("1", "���� ����...");
            response.Add("...");
            response.Add(ColorString("��", colors["red"]));
            foreach(string i in response)
            {
                outputTarget.text += i;
            }
            yield break;
        }

        else
        {
            outputTarget.text = ""; // �ʱ�ȭ

            yield return StartCoroutine(ragHandler.AskServerStream(userInput, (chunk) =>
            {
                Debug.Log("UI ������Ʈ��: " + outputTarget.text);
                outputTarget.text += chunk;
            }));

            onComplete?.Invoke();
            yield break;

            /*
            yield return StartCoroutine(ragHandler.AskServerStream(userInput, (Answer) => {
                string[] answers = Answer.Split(new string[] { "<END>" }, System.StringSplitOptions.None);
                foreach(string item in answers)
                {
                    response.Add(item);
                }
                
            }));

            onComplete(response);
            yield break;*/
        }
    }

    public string ColorString(string s, string color)
    {
        string leftTag = "<color=" + color + ">";
        string rightTag = "</color>";

        return leftTag + s + rightTag;
    }

    void ListEntry(string a, string b)
    {
        response.Add(ColorString(a, colors["red"]) + ColorString(b, colors["cyan"]));
    }

    void LoadTitle(string path, string color, int spacing)
    {
        StreamReader file = new StreamReader(Path.Combine(Application.streamingAssetsPath, path));

        for(int i = 0; i < spacing; i++)
        {
            response.Add("");
        }

        while (!file.EndOfStream)
        {
            response.Add(ColorString(file.ReadLine(), colors[color]));
        }

        for(int i=0; i < spacing; i++)
        {
            response.Add("");
        }

        file.Close();

    }
}
