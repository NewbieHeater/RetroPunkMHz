using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackController : MonoBehaviour
{
    [Header("Attackable Layer")]
    [SerializeField] private LayerMask attackableLayer;

    [Header("UI")]
    public Image chargeBar;
    public GameObject chargeBarParent;
    public TextMeshProUGUI chargedValue;

    [Header("Anim/Hit Window")]
    [SerializeField] private string animTriggerAttack = "Attack";
    [SerializeField] private string animClipCharged = "Attack_5Combo_4_Inplace";
    private Animator animator;

    // 히트 높이(지면에서 얼마 위에서 휘두르는지)
    [SerializeField] private float attackPivotHeight = 1.0f;

    private bool isCharging;
    private float chargeTimer;
    private Camera cam;

    // 비할당/GC 방지
    private readonly Collider[] overlapResults = new Collider[32];

    // 한 번의 공격 동안 중복 타격 방지
    private readonly HashSet<IAttackable> _hitOnce = new HashSet<IAttackable>();

    private IPlayerStats _stats;

    public void Initialize(IPlayerStats stats = null)
    {
        if (!cam) cam = Camera.main;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (stats != null) _stats = stats;
    }

    private (int finalDamage, bool isCrit) ApplyCritical(int baseDamage)
    {
        float chance = _stats != null ? _stats.CritChance : 0f;
        float mult = _stats != null ? _stats.CritMultiplier : 2f;

        bool isCrit = UnityEngine.Random.value < chance;
        int final = isCrit ? Mathf.RoundToInt(baseDamage * mult) : baseDamage;
        return (final, isCrit);
    }

    public void HandleInput()
    {
        var btns = GlobalInputRouter.Instance.CurrentFrame.buttons;

        if (btns.IsDown(InputAction.Attack))
            PerformPrimaryAttack();

        if (btns.IsDown(InputAction.Charge))
            StartCharging();

        // 마우스 직접 체크 대신 입력 라우터에 매핑해 두면 더 깔끔
        if (isCharging && Input.GetMouseButton(1))
            ContinueCharging();

        if (isCharging && Input.GetMouseButtonUp(1))
            PerformChargedAttack();
    }

    public void ProcessAttack() { /* 판정은 ExecuteAttack에서 즉시 처리 */ }

    private void PerformPrimaryAttack()
    {
        if (animator) animator.SetTrigger(animTriggerAttack);

        float amp = ChannelManager.Instance.CurrentChannel.amplitudePoints * 0.1f;
        int scaled = Mathf.RoundToInt(_stats.AttackDamage * (1.0f + amp));

        var (final, isCrit) = ApplyCritical(scaled);

        var info = new DamageInfo
        {
            Amount = final,
            SourceDir = GetAttackDirection(),
            IsCharge = false,
            KnockbackForce = 0f
        };

        ExecuteAttack(info);
    }

    private void StartCharging()
    {
        chargeBarParent?.SetActive(true);
        isCharging = true;
        chargeTimer = 0f;
        UpdateChargeUI(0f);
    }

    private void ContinueCharging()
    {
        if (!isCharging) return;

        chargeTimer = Mathf.Min(chargeTimer + Time.deltaTime, _stats.MaxChargeTime);
        float t = Mathf.InverseLerp(0f, _stats.MaxChargeTime, chargeTimer);
        float dmg = Mathf.Lerp(0f, _stats.MaxChargeTime, t);
        UpdateChargeUI(dmg);
    }

    private void PerformChargedAttack()
    {
        Initialize();

        if (animator) animator.Play(animClipCharged);

        float t = Mathf.Clamp(chargeTimer, 0f, _stats.MaxChargeTime);
        bool charged = t >= _stats.MinChargeTime;

        float lerp = charged
            ? Mathf.InverseLerp(_stats.MinChargeTime, _stats.MaxChargeTime, t)
            : 0f;

        int baseDmg = Mathf.RoundToInt(Mathf.Lerp(_stats.AttackDamage, _stats.ChargeAttackDamage, lerp) / 10f) * 10;

        var (final, isCrit) = ApplyCritical(baseDmg);

        var info = new DamageInfo
        {
            Amount = final,
            SourceDir = GetAttackDirection(),
            IsCharge = charged,
            KnockbackForce = final
        };

        ExecuteAttack(info);
        ResetCharge();
    }

    private void ResetCharge()
    {
        chargeBarParent?.SetActive(false);
        if (chargeBar) chargeBar.fillAmount = 0f;
        if (chargedValue) chargedValue.text = "0";
        isCharging = false;
        chargeTimer = 0f;
    }

    private void UpdateChargeUI(float damagePreview)
    {
        if (chargedValue) chargedValue.text = Mathf.RoundToInt(damagePreview).ToString();
        if (chargeBar) chargeBar.fillAmount = Mathf.Clamp01(chargeTimer / _stats.MaxChargeTime);
    }

    // === 핵심: 공격 판정 (캡슐 축을 dir과 일치) ===
    private void ExecuteAttack(in DamageInfo info)
    {
        _hitOnce.Clear();

        Vector3 dir = GetAttackDirection();
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right;

        // 공격 축(앞/뒤)으로 캡슐 배치
        Vector3 origin = transform.position + Vector3.up * attackPivotHeight;

        // 캡슐의 두 끝점을 dir 방향으로 배치 (리치 길이 사용)
        Vector3 pointA = origin + dir * 0.1f;          // 손/무기 시작 약간 앞
        Vector3 pointB = origin + dir * _stats.AttackRange;   // 끝 리치

        int hitCount = Physics.OverlapCapsuleNonAlloc(
            pointA, pointB, 1,
            overlapResults, attackableLayer,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            if (!overlapResults[i]) continue;

            // 루트에서 IAttackable를 우선 찾음(멀티콜라이더 중복 방지)
            if (!overlapResults[i].TryGetComponent<IAttackable>(out var atk) &&
                !overlapResults[i].TryGetComponent<IAttackable>(out atk))
                continue;

            if (_hitOnce.Contains(atk)) continue;  // 한 번의 스윙에서 한 번만
            _hitOnce.Add(atk);

            atk.TakeDamage(info);
        }
    }

    private Vector3 GetAttackDirection()
    {
        Initialize();
        if (!cam) return Vector3.right;

        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;

        Vector3 world = cam.ScreenToWorldPoint(mp);
        Vector3 dir = world - transform.position;

        // 사이드뷰(XY) 기준
        dir.z = 0f;
        return dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.right;
    }



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 dir = GetAttackDirection();
        Vector3 origin = transform.position + Vector3.up * attackPivotHeight;
        Vector3 a = origin + dir * 0.1f;
        Vector3 b = origin + dir * _stats.AttackRange;

        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.5f);
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(a, Vector3.forward, 1);
        UnityEditor.Handles.DrawWireDisc(b, Vector3.forward, 1);
        Gizmos.DrawLine(a, b);
    }
#endif
}
