using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HeadLookAt : MonoBehaviour
{
    Camera cam;
    [Header("�Ӹ� LookAt ����")]
    public Transform target;              // ���� �� GameObject �ϳ��� �Ҵ�
    [Range(0f, 1f)] public float lookWeight = 1f;
    public float lookClamp = 0.5f;       // Dot < lookClamp �̸� ����
    public float weightBlendSpeed = 2f;  // 1�ʿ� �󸶳� Weight�� �ٲ��� �ӵ�
    public MultiAimConstraint headAimConstraint;

    // ����: �� ������ ���� ���� ���� Weight
    private float currentWeight = 0f;

    void Start()
    {
        cam = Camera.main;
        headAimConstraint = GetComponent<MultiAimConstraint>();
        // �ʱ� weight�� 0���� �ΰų� ���ϴ� ������ ����
        if (headAimConstraint != null)
        {
            currentWeight = headAimConstraint.weight;
        }
    }

    void Update()
    {
        // 1) ���콺 ��ġ �� 2D ���(ray�� Plane ����) �� target �̵�
        Plane lookPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, -1f));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (lookPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            target.position = worldPoint;
        }

        // 2) ĳ���� ���� vs target �������� Dot ���
        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 forward = transform.forward;
        float dot = Vector3.Dot(forward, toTarget);

        // 3) dot�� ���� ��ǥ Weight ����
        float desiredWeight = (dot < lookClamp) ? 0f : lookWeight;

        // 4) ���� Weight���� ��ǥ Weight�� �ε巴�� ����
        //    Mathf.MoveTowards�� ���� ���� �ӵ��� ����/�����մϴ�.
        currentWeight = Mathf.MoveTowards(
            currentWeight,
            desiredWeight,
            weightBlendSpeed * Time.deltaTime
        );

        // 5) Rigging MultiAimConstraint�� ����
        if (headAimConstraint != null)
        {
            headAimConstraint.weight = currentWeight;
        }
    }
}
