using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthBarView : MonoBehaviour
{

    [SerializeField] private EnemyHealth source;
    [SerializeField] TextMeshProUGUI hpText;

    [Header("Smoothing")]
    [SerializeField] bool smooth = true;

    float _target01, _current01;

    Transform cam;
    Transform parentTf;

    void Awake()
    {
        if (!source) source = GetComponentInParent<EnemyHealth>();
        if (!hpText) hpText = GetComponentInChildren<TextMeshProUGUI>();
        cam = Camera.main.transform;
        parentTf = transform.parent;      // 헬스바가 캐릭터 자식이라면
    }
    void OnEnable()
    {
        if (!source) return;
        source.OnHpChangedEx += HandleHpChanged;
        // 초기 동기화
        HandleHpChanged(source.Hp, source.MaxHp, source.MaxHp > 0 ? source.Hp / source.MaxHp : 0f);
    }

    void OnDisable()
    {
        if (!source) return;
        source.OnHpChangedEx -= HandleHpChanged;
    }

    void HandleHpChanged(float cur, float max, float norm)
    {
        _target01 = norm;
        if (hpText)
        {
            // GC 최소화를 위해 SetText 사용
            hpText.SetText("{0}/{1}", Mathf.CeilToInt(cur), Mathf.CeilToInt(max));
        }
        if (!smooth)
        {
            _current01 = _target01;
            Apply(_current01);
        }
    }
    void Apply(float v01)
    {
        //if (slider) slider.value = v01;
        //if (fill) fill.fillAmount = v01;
        // 필요하면 그라데이션/색 보간도 여기서
        // fill.color = Color.Lerp(Color.red, Color.green, v01);
    }

    void LateUpdate()
    {
        Vector3 lookDir = cam.forward;

        Vector3 worldUp = parentTf.up;

        transform.LookAt(transform.position + lookDir, worldUp);
    }
}
