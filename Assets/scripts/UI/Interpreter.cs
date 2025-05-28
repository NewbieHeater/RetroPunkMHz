using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpreter : MonoBehaviour
{
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

    public List<string> Interpret(string userInput)
    {
        response.Clear();

        string[] args = userInput.Split();

        if(args[0] == "/help")
        {
            response.Add("�𸣴� ���� ������ �͹̳ο��� �������.");
            response.Add("Ŀ�ǵ带 ����Ϸ��� \'/\'�ڿ� ��ɾ �Է��ϼ���");
            return response;
        }
        else if(args[0] == "/selfDestroy")
        {
            response.Add("���� ������ ����");
            ListEntry("3", "���� ����...");
            ListEntry("2", "���� ����...");
            ListEntry("1", "���� ����...");
            response.Add("...");
            response.Add(ColorString("��",colors["red"]));
            return response;
        }
        else
        {
            return response;
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
}
