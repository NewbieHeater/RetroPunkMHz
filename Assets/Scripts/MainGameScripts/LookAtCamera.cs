using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform cam;
    Transform parentTf;

    void Start()
    {
        cam = Camera.main.transform;
        parentTf = transform.parent;      // �ｺ�ٰ� ĳ���� �ڽ��̶��
    }

    void LateUpdate()
    {
        // �� �ٶ� ����: ī�޶� �ٶ󺸴� ����� �����ϰ�
        Vector3 lookDir = cam.forward;

        // �� up ���ͷ� �θ�(ĳ����)�� up�� �Ѱ��ֱ�
        Vector3 worldUp = parentTf.up;

        // �� ���� ȸ�� ����
        transform.LookAt(transform.position + lookDir, worldUp);
    }
}