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
            response.Add("모르는 것이 있으면 터미널에게 물어보세요.");
            response.Add("커맨드를 사용하려면 \'/\'뒤에 명령어를 입력하세요");
            return response;
        }
        else if(args[0] == "/selfDestroy")
        {
            response.Add("자폭 시퀀스 가동");
            ListEntry("3", "초후 폭발...");
            ListEntry("2", "초후 폭발...");
            ListEntry("1", "초후 폭발...");
            response.Add("...");
            response.Add(ColorString("붐",colors["red"]));
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
