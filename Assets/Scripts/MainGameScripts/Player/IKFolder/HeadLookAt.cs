using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HeadLookAt : MonoBehaviour
{
    Camera cam;
    [Header("머리 LookAt 설정")]
    public Transform target;              // 씬에 빈 GameObject 하나를 할당
    [Range(0f, 1f)] public float lookWeight = 1f;
    public float lookClamp = 0.5f;       // Dot < lookClamp 이면 무시
    public float weightBlendSpeed = 2f;  // 1초에 얼마나 Weight를 바꿀지 속도
    public MultiAimConstraint headAimConstraint;

    // 내부: 매 프레임 현재 적용 중인 Weight
    private float currentWeight = 0f;

    void Start()
    {
        cam = Camera.main;
        headAimConstraint = GetComponent<MultiAimConstraint>();
        // 초기 weight는 0으로 두거나 원하는 값으로 세팅
        if (headAimConstraint != null)
        {
            currentWeight = headAimConstraint.weight;
        }
    }

    void Update()
    {
        // 1) 마우스 위치 → 2D 평면(ray와 Plane 교차) → target 이동
        Plane lookPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, -1f));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (lookPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            target.position = worldPoint;
        }

        // 2) 캐릭터 정면 vs target 방향으로 Dot 계산
        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 forward = transform.forward;
        float dot = Vector3.Dot(forward, toTarget);

        // 3) dot에 따라 목표 Weight 결정
        float desiredWeight = (dot < lookClamp) ? 0f : lookWeight;

        // 4) 현재 Weight에서 목표 Weight로 부드럽게 보간
        //    Mathf.MoveTowards를 쓰면 일정 속도로 증가/감소합니다.
        currentWeight = Mathf.MoveTowards(
            currentWeight,
            desiredWeight,
            weightBlendSpeed * Time.deltaTime
        );

        // 5) Rigging MultiAimConstraint에 적용
        if (headAimConstraint != null)
        {
            headAimConstraint.weight = currentWeight;
        }
    }
}
