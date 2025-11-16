using UnityEngine;

[CreateAssetMenu(menuName = "Story/Main Stream Condition Flag")]
public class MainStreamConditionFlag : ScriptableObject
{
    [SerializeField] private string _conditionName;
    [SerializeField] private bool _isActive;

    public string ConditionName => _conditionName;
    public bool IsActive => _isActive;

    /// <summary>게임 플레이 중 조건을 만족했을 때 호출</summary>
    public void Activate()
    {
        if (_isActive) return;

        _isActive = true;

        if (MainStreamManager.HasInstance)
            MainStreamManager.Instance.OnConditionActivated(this);
    }

    public void ResetFlag()
    {
        _isActive = false;
    }

    /// <summary>세이브 데이터 로드시 전용 (콜백 없이 상태만 복원)</summary>
    public void SetFromSave(bool active)
    {
        _isActive = active;
    }
}
