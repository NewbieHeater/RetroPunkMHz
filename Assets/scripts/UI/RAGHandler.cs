using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;

public class RAGHandler : MonoBehaviour
{
    [System.Serializable]
    public class AskRequest
    {
        public string question;
    }

    [System.Serializable]
    public class AskResponse
    {
        public string answer;
        public string[] context;
    }

    public class StreamingHandler : DownloadHandlerScript
    {
        System.Action<string> onChunk;
        private StringBuilder buffer = new StringBuilder();

        public StreamingHandler(System.Action<string> onChunkCallback) : base()
        {
            onChunk = onChunkCallback;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length == 0) return false;

            string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
            buffer.Append(chunk);

            //�ٹٲ� �������� chunk�ɰ���
            string[] lines = buffer.ToString().Split('\n');

            for (int i = 0; i < lines.Length - 1; i++)
            {
                onChunk?.Invoke(lines[i]);
            }

            buffer.Clear();
            buffer.Append(lines[lines.Length - 1]);// ������ ���� �� �ϼ��� ��Ʈ���̹Ƿ� ����

            return true;
        }

        protected override void CompleteContent()
        {
            if(buffer.Length > 0)
            {
                onChunk?.Invoke(buffer.ToString());
                buffer.Clear();
            }
        }
    }


    public IEnumerator AskServer(string question, System.Action<string,string[]> onResponse)
    {
        AskRequest requestData = new AskRequest { question = question };
        string json = JsonUtility.ToJson(requestData);
        Debug.Log("������ JSON: " + json);

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
            Debug.LogError("���� �߻�: " + request.error);
        }
    }

    public IEnumerator AskServerStream(string question, System.Action<string> onTextStream)
    {
        AskRequest requestData = new AskRequest { question = question };
        string json = JsonUtility.ToJson(requestData);
        Debug.Log("������ JSON: " + json);

        UnityWebRequest request = new UnityWebRequest("http://localhost:5000/ask-stream", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new StreamingHandler(chunk =>
        {
            Debug.Log("���� ûũ: " + chunk);  // �ǽð� �α� Ȯ��
            onTextStream?.Invoke(chunk);
        });
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("��Ʈ���� ����: " + request.error);
    }

}
