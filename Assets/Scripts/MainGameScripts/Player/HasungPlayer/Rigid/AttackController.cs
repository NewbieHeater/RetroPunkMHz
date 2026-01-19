using Game.Controls;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackController : MonoBehaviour
{
    [Header("Attackable Layer")]
    [SerializeField] private LayerMask attackableLayer;

    [Header("UI")]
    [SerializeField] private Image chargeBar;
    [SerializeField] private GameObject chargeBarParent;
    [SerializeField] private TextMeshProUGUI chargedValue;

    [Header("Anim/Hit Window")]
    [SerializeField] private string animTriggerAttack = "Attack";
    [SerializeField] private string animClipCharged = "Attack_5Combo_4_Inplace";
    private Animator animator;

    [Header("Hit Shape")]
    [SerializeField] private float attackPivotHeight = 1.0f;
    [SerializeField] private float hitRadius = 0.6f;

    [Header("Attack Speed Apply")]
    [Tooltip("기준 쿨다운(초). 실제 쿨다운은 base / AttackSpeed 입니다.")]
    [SerializeField] private float baseAttackCooldown = 0.5f; // 1초에 2타 기준
    [Tooltip("차지 속도에도 AttackSpeed를 곱해 차지 시간을 단축시킬지 여부")]
    [SerializeField] private bool chargeAffectedByAttackSpeed = false;

    private bool isCharging;
    private float chargeTimer;
    private Camera cam;

    // 비할당/GC 방지
    private readonly Collider[] overlapResults = new Collider[32];
    // 한 번의 공격 동안 중복 타격 방지
    private readonly HashSet<IAttackable> _hitOnce = new HashSet<IAttackable>();

    private PlayerStats _stats;

    // 쿨다운 관리
    private float nextAttackTime;
    private float EffectiveCooldown =>
        Mathf.Max(0.01f, baseAttackCooldown / Mathf.Max(0.01f, _stats != null ? _stats.AttackSpeed : 1f));

    public void Initialize(PlayerStats stats = null)
    {
        // 한 번만 세팅
        if (!cam) cam = Camera.main;
        if (!animator) animator = GetComponentInChildren<Animator>();
        _stats = stats;
    }

    private void Awake()
    {
        Initialize(); // 조기 캐싱
    }

    // 치명타 로직: 확률의 절댓값으로 발동, 양수면 ×CritMultiplier, 음수면 ×0.75
    private (int finalDamage, bool activated) ApplyCritical(int baseDamage)
    {
        if (_stats == null)
            return (baseDamage, false);

        float chance = _stats.CritChance;          // 양/음수 가능
        float absChance = Mathf.Abs(chance);       // 발동 확률은 절댓값
        float mult = 1f;
        bool activated = false;

        if (Random.value < absChance)
        {
            activated = true;
            mult = (chance > 0f) ? _stats.CritMultiplier : 0.75f;
        }

        int final = Mathf.RoundToInt(baseDamage * mult);
        return (final, activated);
    }

    public void HandleInput()
    {
        var frame = GlobalInputRouter.Instance.CurrentFrame;
        var btns = frame.buttons;

        // 애니메이터 파라미터(있으면 사용)
        if (animator)
        {
            float spd = _stats != null ? _stats.AttackSpeed : 1f;
            animator.SetFloat("AttackSpeed", spd); // Animator Controller에 "AttackSpeed" 파라미터 추가 권장
        }

        // 기본 공격 (쿨다운 체크)
        if (btns.IsDown(GameInputAction.Attack) && Time.time >= nextAttackTime)
            PerformPrimaryAttack();

        // 차지 시작
        if (btns.IsDown(GameInputAction.Charge))
            StartCharging();

        // 차지 유지
        bool holdCharge = btns.IsHeld(GameInputAction.Charge);
        if (isCharging && holdCharge)
            ContinueCharging();

        // 차지 해제/발동
        bool releaseCharge = btns.IsUp(GameInputAction.Charge);
        if (isCharging && releaseCharge)
            PerformChargedAttack();
    }

    public void ProcessAttack() { /* 판정은 ExecuteAttack에서 즉시 처리 */ }

    private void PerformPrimaryAttack()
    {
        if (animator) animator.SetTrigger(animTriggerAttack);
        
        int scaled = Mathf.RoundToInt(_stats != null ? _stats.AttackDamage : 10f);
        var (final, crit) = ApplyCritical(scaled);
        var info = new DamageInfo
        {
            Amount = final,
            SourceDir = GetAttackDirection(),
            IsCharge = false,
            KnockbackForce = 1f
        };

        ExecuteAttack(info);

        // 공격 후 쿨타임 갱신
        nextAttackTime = Time.time + EffectiveCooldown;
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
        if (!isCharging || _stats == null) return;

        float mult = (chargeAffectedByAttackSpeed && _stats != null) ? _stats.AttackSpeed : 1f;
        chargeTimer = Mathf.Min(chargeTimer + Time.deltaTime * mult, _stats.MaxChargeTime);

        float t = Mathf.InverseLerp(0f, _stats.MaxChargeTime, chargeTimer);
        float preview = Mathf.Lerp(_stats.AttackDamage, _stats.ChargeAttackDamage, t);
        UpdateChargeUI(preview);
    }

    private void PerformChargedAttack()
    {
        if (_stats == null) { ResetCharge(); return; }

        if (animator) animator.Play(animClipCharged);

        float t = Mathf.Clamp(chargeTimer, 0f, _stats.MaxChargeTime);
        bool charged = t >= _stats.MinChargeTime;

        float lerp = charged
            ? Mathf.InverseLerp(_stats.MinChargeTime, _stats.MaxChargeTime, t)
            : 0f;

        int baseDmg = Mathf.RoundToInt(Mathf.Lerp(_stats.AttackDamage, _stats.ChargeAttackDamage, lerp) / 10f) * 10;
        var (final, _) = ApplyCritical(baseDmg);

        var info = new DamageInfo
        {
            Amount = final,
            SourceDir = GetAttackDirection(),
            IsCharge = charged,
            KnockbackForce = final /3
        };

        ExecuteAttack(info);
        ResetCharge();

        // 차지 공격 후에도 쿨타임 적용 (원하면 배수를 두어도 됨)
        nextAttackTime = Time.time + EffectiveCooldown;
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
        if (_stats != null && chargeBar)
            chargeBar.fillAmount = Mathf.Clamp01(chargeTimer / _stats.MaxChargeTime);

        if (chargedValue)
            chargedValue.text = Mathf.RoundToInt(damagePreview).ToString();
    }

    // === 핵심: 공격 판정 ===
    private void ExecuteAttack(in DamageInfo info)
    {
        _hitOnce.Clear();

        Vector3 dir = GetAttackDirection();
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right;

        Vector3 origin = transform.position + Vector3.up * attackPivotHeight;
        Vector3 pointA = origin + dir * 0.1f; // 시작점(손 앞)
        float range = _stats != null ? _stats.AttackRange : 1f;
        Vector3 pointB = origin + dir * range; // 끝 리치

        int hitCount = Physics.OverlapCapsuleNonAlloc(
            pointA, pointB, hitRadius,
            overlapResults, attackableLayer,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (!col) continue;

            // LivingEntity와 Collider가 같은 객체라고 가정 → TryGetComponent 한 번만
            if (!col.TryGetComponent<IAttackable>(out var atk))
                continue;

            if (_hitOnce.Contains(atk)) continue; // 한 스윙 1히트
            _hitOnce.Add(atk);

            atk.TakeDamage(info);
            
        }
    }

    private Vector3 GetAttackDirection()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return Vector3.right;

        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;

        Vector3 world = cam.ScreenToWorldPoint(mp);
        Vector3 dir = world - transform.position;

        dir.z = 0f; // 2D(사이드뷰) 가정
        return dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.right;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        float range = _stats != null ? _stats.AttackRange : 1f;

        Vector3 dir = GetAttackDirection();
        Vector3 origin = transform.position + Vector3.up * attackPivotHeight;
        Vector3 a = origin + dir * 0.1f;
        Vector3 b = origin + dir * range;

        Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.35f);
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(a, Vector3.forward, hitRadius);
        UnityEditor.Handles.DrawWireDisc(b, Vector3.forward, hitRadius);
        Gizmos.DrawLine(a, b);
    }
#endif
}
