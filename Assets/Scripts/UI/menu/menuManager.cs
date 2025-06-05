using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menuManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public int totalPages = 3;
    public float transitionSpeed = 5f;

    [SerializeField] private int currentPage = 0;
    private float targetPos;

    void Update()
    {
        // �¿� Ű �Է�
        if (Input.GetKeyDown(KeyCode.Q)) SlideToPage(currentPage - 1);
        if (Input.GetKeyDown(KeyCode.E)) SlideToPage(currentPage + 1);

        // �ε巴�� �̵�
        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            targetPos,
            Time.deltaTime * transitionSpeed
        );
    }

    void SlideToPage(int pageIndex)
    {
        Debug.Log("kb hit");
        currentPage = Mathf.Clamp(pageIndex, 0, totalPages - 1);
        targetPos = (float)currentPage / (totalPages - 1); // 0.0 ~ 1.0 ���� ��
    }
}
