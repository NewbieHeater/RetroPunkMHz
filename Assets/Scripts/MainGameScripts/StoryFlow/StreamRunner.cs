using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public class StreamRunner : MonoBehaviour
{
    [Tooltip("게임 시작 시 재생할 초기 스트림")]
    public StoryStream initialStream;

    private bool isBusy;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (GameProgress.I.currentStream == null && initialStream != null)
            StartStream(initialStream);
    }

    private void OnEnable()
    {
        if (GameProgress.I != null)
            GameProgress.I.FlagsChanged += Evaluate;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (GameProgress.I != null)
            GameProgress.I.FlagsChanged -= Evaluate;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Evaluate();
    }

    public void StartStream(StoryStream stream)
    {
        if (stream == null) return;

        GameProgress.I.currentStream = stream;
        GameProgress.I.currentNodeIndex = 0;

        if (stream.nodes.Count > 0)
            EnterNode(stream.nodes[0]);
        else
            ResolveAndStartNextStream(stream);
    }

    private void EnterNode(Node node)
    {
        if (node == null) return;
        StartCoroutine(EnterNodeRoutine(node));
    }

    private IEnumerator EnterNodeRoutine(Node node)
    {
        // 1) onEnter 대사
        if (node.playDialogueOnEnter &&
            DialogueManager.Instance != null &&
            !string.IsNullOrEmpty(node.enterDialogueFile) &&
            !string.IsNullOrEmpty(node.enterDialogueGroup))
        {
            if (node.waitEnterDialogue)
                yield return DialogueManager.Instance.StartDialogueAndWait(
                    node.enterDialogueFile, node.enterDialogueGroup);
            else
                DialogueManager.Instance.StartDialogue(
                    node.enterDialogueFile, node.enterDialogueGroup);
        }

        // 2) 기타 onEnter 이벤트(SO)
        yield return RunEventsSequentially(node.onEnter);

        // 3) 노드 시작 직후 평가
        Evaluate();
    }

    public void Evaluate()
    {
        if (isBusy) return;
        StartCoroutine(EvaluateRoutine());
    }

    private IEnumerator EvaluateRoutine()
    {
        if (isBusy) yield break;
        isBusy = true;

        var stream = GameProgress.I.currentStream;
        if (stream == null) { isBusy = false; yield break; }

        int index = GameProgress.I.currentNodeIndex;
        if (index < 0 || index >= stream.nodes.Count)
        {
            ResolveAndStartNextStream(stream);
            isBusy = false;
            yield break;
        }

        var node = stream.nodes[index];
        string nodeKey = $"{stream.name}:{index}";

        // ---- Task 처리 ----
        foreach (var task in node.tasks)
        {
            if (GameProgress.I.IsTaskCompleted(nodeKey, task.id)) continue;

            bool ready = task.conditions.All(c => c != null && c.IsMet());
            if (ready)
            {
                yield return RunEventsSequentially(task.onClear);
                GameProgress.I.MarkTaskCompleted(nodeKey, task.id);
            }
        }

        // ---- 노드 완료 판정 ----
        bool condOk = node.conditions.All(c => c != null && c.IsMet());
        bool tasksOk = node.tasks.All(t => GameProgress.I.IsTaskCompleted(nodeKey, t.id));

        if (condOk && tasksOk)
        {
            // 1) onClear 대사
            if (node.playDialogueOnClear &&
                DialogueManager.Instance != null &&
                !string.IsNullOrEmpty(node.clearDialogueFile) &&
                !string.IsNullOrEmpty(node.clearDialogueGroup))
            {
                if (node.waitClearDialogue)
                    yield return DialogueManager.Instance.StartDialogueAndWait(
                        node.clearDialogueFile, node.clearDialogueGroup);
                else
                    DialogueManager.Instance.StartDialogue(
                        node.clearDialogueFile, node.clearDialogueGroup);
            }

            // 2) 기타 onClear 이벤트
            yield return RunEventsSequentially(node.onClear);

            // 3) 다음 노드로 직진
            int nextIndex = index + 1;
            if (nextIndex < stream.nodes.Count)
            {
                GameProgress.I.currentNodeIndex = nextIndex;
                EnterNode(stream.nodes[nextIndex]);
            }
            else
            {
                ResolveAndStartNextStream(stream);
            }
        }

        isBusy = false;
    }


    private void ResolveAndStartNextStream(StoryStream stream)
    {
        StoryStream next = null;

        if (stream.nextStreamRules != null)
        {
            foreach (var rule in stream.nextStreamRules)
            {
                if (rule != null && rule.nextStream != null &&
                    rule.conditions.All(c => c != null && c.IsMet()))
                {
                    next = rule.nextStream;
                    break;
                }
            }
        }

        if (next == null)
            next = stream.defaultNextStream;

        if (next != null)
            StartStream(next);
    }

    private IEnumerator RunEventsSequentially(System.Collections.Generic.List<GameEventSO> events)
    {
        if (events == null) yield break;

        foreach (var ev in events)
        {
            if (ev == null) continue;

            if (ev is IAsyncGameEvent asyncEv)
                yield return StartCoroutine(asyncEv.InvokeRoutine());
            else
            {
                ev.Invoke();
                yield return null;
            }
        }
    }
}
