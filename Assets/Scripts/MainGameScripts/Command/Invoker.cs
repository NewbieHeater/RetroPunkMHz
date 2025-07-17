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

    // 녹화 중인 경우: recordingTime, 재생 중인 경우: replayTime
    public float CurrentTime => IsRecording ? recordingTime : replayTime;

    void Update()
    {
        // 테스트용: R 키로 녹화 시작/종료, T 키로 재생 시작
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
        Debug.Log("녹화 시작");
    }

    public void StopRecording()
    {
        IsRecording = false;
        Debug.Log("녹화 종료");
    }

    public void StartReplay()
    {
        replayTime = 0f;
        IsReplaying = true;
        IsRecording = false;
        if (recordedCommands.Count <= 0)
            Debug.LogError("No commands to replay!");

        recordedCommands.Reverse();
        Debug.Log("재생 시작");
    }

    public void StopReplay()
    {
        IsReplaying = false;
        Debug.Log("재생 종료");
    }

    /// <summary>
    /// 녹화 중이면 time 설정 후 리스트에 추가, 
    /// 재생 중이 아니면 즉시 Execute()
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

            // 씬에 있는 모든 Recordable 오브젝트 위치/회전 녹화
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
            // 가장 작은 키(시간)가 replayTime 이하인 동안 반복
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
