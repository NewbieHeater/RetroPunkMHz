using UnityEngine;

public static class DialogueEventRouter
{
    public static void Dispatch(EventInfo ev)
    {
        if (ev == null) return;

        switch (ev.type)
        {
            case "flag":
                // ev.id = 플래그명, ev.action = "set"/"unset"
                bool set = ev.action != "unset";
                GameProgress.I.SetFlag(ev.id, set);
                break;

            case "quest":
                // 예: id="Burger", action="accept"/"complete"
                GameProgress.I.SetFlag($"quest.{ev.id}.{ev.action}", true);
                break;

            case "removeEntity":
                // ev.id = entityGuid
                if (!string.IsNullOrEmpty(ev.id))
                    GameProgress.I.RemoveEntity(ev.id);
                break;

            case "gate":
                // 문 여닫기 등은 프로젝트 규칙에 맞게 연결
                // 예: GameObject.Find(ev.target)?.GetComponent<YourGate>()?.Open();
                break;

            case "item":
                // 인벤토리 시스템과 연결
                // Inventory.Give(ev.id, ev.amount);
                break;
        }

        // 플래그 변동 등으로 즉시 재평가
        var runner = Object.FindObjectOfType<StreamRunner>();
        runner?.Evaluate();
    }

    // string eventKey도 지원 (line.eventKey 용)
    public static void Dispatch(string eventKey)
    {
        if (string.IsNullOrEmpty(eventKey)) return;

        // 간단 프리픽스 규칙 예시
        if (eventKey.StartsWith("flag:"))
        {
            var key = eventKey.Substring("flag:".Length);
            GameProgress.I.SetFlag(key, true);
        }
        else if (eventKey.StartsWith("removeEntity:"))
        {
            var guid = eventKey.Substring("removeEntity:".Length);
            if (!string.IsNullOrEmpty(guid)) GameProgress.I.RemoveEntity(guid);
        }
        else if (eventKey == "OpenShop")
        {
            // 기존 처리 유지
        }
        // 필요시 else if 추가…

        var runner = Object.FindObjectOfType<StreamRunner>();
        runner?.Evaluate();
    }
}
