using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackController : MonoBehaviour
{
    [Header("Attackable Layer")]
    [SerializeField] private LayerMask attackableLayer;
    [Header("Attack Settings")]
    [SerializeField] private int normalDamage = 10;
    [SerializeField] private float minChargeTime = 0.2f, maxChargeTime = 1f;
    [SerializeField] private float maxChargeDamage = 30f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 0.5f;

    public Image chargeBar;
    public GameObject chargeBarParent;
    public TextMeshProUGUI chargedValue;

    private bool isCharging;
    private float chargeTimer;
    private Camera cam;
    private Animator animator;
    private Collider[] overlapResults = new Collider[16];
    private float attackCapsuleHeight = 1f;

    public void Initialize()
    {
        cam = Camera.main;
        animator = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        HandleInput();
    }

    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) PerformPrimaryAttack();
        if (Input.GetMouseButtonDown(1)) StartCharging();
        if (isCharging && Input.GetMouseButton(1)) ContinueCharging();
        if (isCharging && Input.GetMouseButtonUp(1)) PerformChargedAttack();
    }

    public void ProcessAttack()
    {
        // Attack overlaps are resolved immediately upon PerformPrimaryAttack / PerformChargedAttack calls
    }

    private void PerformPrimaryAttack()
    {

        animator.SetTrigger("Attack");
        DamageInfo info = new DamageInfo { Amount = normalDamage, SourceDir = GetAttackDirection(), IsCharge = false, KnockbackForce = 0f };
        ExecuteAttack(info);
    }

    private void StartCharging()
    {
        chargeBarParent.SetActive(true);
        isCharging = true;
        chargeTimer = 0f;
    }

    private void ContinueCharging()
    {
        if (chargeTimer >= maxChargeTime) return;
        chargeTimer += Time.deltaTime;
        float damage = Mathf.FloorToInt((chargeTimer * maxChargeDamage) / maxChargeTime);
        chargedValue.text = damage.ToString();
        chargeBar.fillAmount = chargeTimer / maxChargeTime;
    }

    private void PerformChargedAttack()
    {
        animator.Play("Attack_5Combo_4_Inplace");
        float t = Mathf.Clamp(chargeTimer, minChargeTime, maxChargeTime);
        int dmg = (int)Mathf.Lerp(normalDamage, maxChargeDamage, (t - minChargeTime) / (maxChargeTime - minChargeTime)) /10 *10;
        DamageInfo info = new DamageInfo { Amount = dmg, SourceDir = GetAttackDirection(), IsCharge = t >= minChargeTime, KnockbackForce = dmg };
        ExecuteAttack(info);
        ResetCharge();
    }

    private void ResetCharge()
    {
        chargeBarParent.SetActive(false);
        chargeBar.fillAmount = 0f;
        isCharging = false;
    }

    private void ExecuteAttack(in DamageInfo info)
    {
        Vector3 dir = GetAttackDirection();
        Vector3 tipCenter = transform.position + Vector3.up + dir * attackRange / 2f;
        attackRadius = attackRange / 2f;
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f).normalized;
        float halfHeight = attackCapsuleHeight * 0.5f;
        Vector3 pointA = tipCenter + perp * halfHeight;
        Vector3 pointB = tipCenter - perp * halfHeight;
        int hitCount = Physics.OverlapCapsuleNonAlloc(pointA, pointB, attackRadius, overlapResults, attackableLayer, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hitCount; i++) if (overlapResults[i].TryGetComponent<IAttackable>(out var atk)) atk.TakeDamage(info);
    }

    private Vector3 GetAttackDirection()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = cam.WorldToScreenPoint(transform.position).z;
        Vector3 world = cam.ScreenToWorldPoint(mp);
        Vector3 dir = (world - transform.position);
        dir.z = 0f;
        return dir.normalized;
    }
}
