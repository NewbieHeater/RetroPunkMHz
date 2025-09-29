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
        parentTf = transform.parent;      // 헬스바가 캐릭터 자식이라면
    }

    void LateUpdate()
    {
        // ① 바라볼 방향: 카메라가 바라보는 방향과 동일하게
        Vector3 lookDir = cam.forward;

        // ② up 벡터로 부모(캐릭터)의 up을 넘겨주기
        Vector3 worldUp = parentTf.up;

        // ③ 월드 회전 설정
        transform.LookAt(transform.position + lookDir, worldUp);
    }
}