using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RAGHandler : MonoBehaviour
{
    [System.Serializable]
    public class AskReequest
    {
        public string question;
    }

    [System.Serializable]
    public class AskResponse
    {
        public string answer;
        public string[] context;
    }

    public IEnumerator AskServer(string question, System.Action<string,string[]> onResponse)
    {
        AskReequest requestData = new AskReequest { question = question };
        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest("http://localhost:5000/ask", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            AskResponse response = JsonUtility.FromJson<AskResponse>(jsonResult);
            onResponse?.Invoke(response.answer, response.context);
        }
        else
        {
            Debug.LogError("오류 발생: " + request.error);
        }
    }
}
