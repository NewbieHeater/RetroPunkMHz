// 상호작용 가능한 객체들이 구현할 인터페이스
public interface IInteractable
{
    void Interact();                     // 상호작용 동작을 실행
    string GetInteractPrompt();          // (옵션) 상호작용 힌트 메시지 제공
}
