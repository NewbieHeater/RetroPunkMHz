using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Interpreter : MonoBehaviour
{
    RAGHandler ragHandler;
    string answer;
    public float printDelay;
    public float printDelayLine;
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
            outputTarget.text = ""; // �ʱ�ȭ

            response.Add("�𸣴� ���� ������ �͹̳ο��� �������.");
            response.Add("Ŀ�ǵ带 ����Ϸ��� \'/\'�ڿ� ��ɾ �Է��ϼ���");
            yield return StartCoroutine(PrintSequentialy(outputTarget));
            onComplete?.Invoke();
            yield break;
        }

        else if (args[0] == "/ascii")
        {
            outputTarget.text = ""; // �ʱ�ȭ

            LoadTitle("ascii.txt", "cyan", 2);
            yield return StartCoroutine(PrintSequentialyLine(outputTarget));
            onComplete?.Invoke();
            yield break;
        }

        else if (args[0] == "/selfDestroy")
        {
            outputTarget.text = ""; // �ʱ�ȭ

            response.Add("���� ������ ����");
            ListEntry("3", "���� ����...");
            ListEntry("2", "���� ����...");
            ListEntry("1", "���� ����...");
            response.Add("...");
            response.Add(ColorString("��", colors["red"]));
            StartCoroutine(PrintSequentialy(outputTarget));
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

        }
    }

    IEnumerator PrintSequentialy(Text outputTarget)
    {
        int cnt = 0;
        foreach(string line in response)
        {
            foreach (char i in line)
            {
                outputTarget.text += i;
                yield return new WaitForSeconds(printDelay);
            }
            if(cnt < response.Count-1)
                outputTarget.text += "\n";
            cnt++;
        }
    }
    IEnumerator PrintSequentialyLine(Text outputTarget)
    {
        int cnt = 0;
        foreach (string line in response)
        {
            outputTarget.text += line;
            yield return new WaitForSeconds(printDelayLine);
            if (cnt < response.Count - 1)
                outputTarget.text += "\n";
            cnt++;
        }
    }

    public string ColorString(string s, string color)
    {
        string Taged = "<color=" + color + ">" + s + "</color>";

        return Taged;
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
