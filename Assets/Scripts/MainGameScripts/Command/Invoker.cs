using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Invoker : MonoBehaviour
{
    public bool IsRecording { get; private set; }
    public bool IsReplaying { get; private set; }

    float recordingTime;
    float replayTime;

    private SortedList<float, List<Command>> recordedCommands
        = new SortedList<float, List<Command>>();

    // ��ȭ ���� ���: recordingTime, ��� ���� ���: replayTime
    public float CurrentTime => IsRecording ? recordingTime : replayTime;

    void Update()
    {
        // �׽�Ʈ��: R Ű�� ��ȭ ����/����, T Ű�� ��� ����
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!IsRecording) StartRecording();
            else StopRecording();
        }
        if (Input.GetKeyDown(KeyCode.T) && !IsReplaying)
            StartReplay();
    }

    public void StartRecording()
    {
        recordedCommands.Clear();
        ReplayRegistry.Clear();
        recordingTime = 0f;
        IsRecording = true;
        IsReplaying = false;
        Debug.Log("��ȭ ����");
    }

    public void StopRecording()
    {
        IsRecording = false;
        Debug.Log("��ȭ ����");
    }

    public void StartReplay()
    {
        replayTime = 0f;
        IsReplaying = true;
        IsRecording = false;
        if (recordedCommands.Count <= 0)
            Debug.LogError("No commands to replay!");

        recordedCommands.Reverse();
        Debug.Log("��� ����");
    }

    public void StopReplay()
    {
        IsReplaying = false;
        Debug.Log("��� ����");
    }

    /// <summary>
    /// ��ȭ ���̸� time ���� �� ����Ʈ�� �߰�, 
    /// ��� ���� �ƴϸ� ��� Execute()
    /// </summary>
    public void RecordAndExecute(Command cmd)
    {
        if (IsRecording)
        {
            cmd.time = recordingTime;
            if (!recordedCommands.ContainsKey(cmd.time))
                recordedCommands[cmd.time] = new List<Command>();
            recordedCommands[cmd.time].Add(cmd);
        }
        if (!IsReplaying)
            cmd.Execute();
    }
    public void Record(Command cmd)
    {
        if (IsRecording)
        {
            cmd.time = recordingTime;
            if (!recordedCommands.ContainsKey(cmd.time))
                recordedCommands[cmd.time] = new List<Command>();
            recordedCommands[cmd.time].Add(cmd);
        }
    }

    void FixedUpdate()
    {
        if (IsRecording)
        {
            recordingTime += Time.fixedDeltaTime;

            // ���� �ִ� ��� Recordable ������Ʈ ��ġ/ȸ�� ��ȭ
            foreach (var rec in FindObjectsOfType<Recordable>())
            {
                var tc = new TransformCommands(
                    rec.InstanceID,
                    rec.transform.position,
                    rec.transform.rotation
                );
                tc.time = recordingTime;

                if (!recordedCommands.ContainsKey(tc.time))
                    recordedCommands[tc.time] = new List<Command>();

                recordedCommands[tc.time].Add(tc);
            }
        }

        if (IsReplaying)
        {
            replayTime += Time.fixedDeltaTime;
            // ���� ���� Ű(�ð�)�� replayTime ������ ���� �ݺ�
            while (recordedCommands.Count > 0
                   && recordedCommands.Keys[0] <= replayTime)
            {
                var key = recordedCommands.Keys[0];
                var cmdList = recordedCommands.Values[0];
                foreach (var cmd in cmdList)
                    cmd.Execute();
                recordedCommands.RemoveAt(0);
            }
            if (recordedCommands.Count == 0)
                StopReplay();
        }
    }
}
