using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menuManager : MonoBehaviour
{
    public KeyCode activationKey;

    public ScrollRect scrollRect;
    public int totalPages = 3;
    public float transitionSpeed = 5f;
    public bool active;

    [SerializeField] private int currentPage;
    [SerializeField] private int startPage;
    private float targetPos;

    public RectTransform panel;
    public Vector2 hiddenPos;  // ������ ��ġ (����)
    public Vector2 shownPos;     // ���̴� ��ġ
    public Vector2 verticalTartgetPos;

    private void Start()
    {
        panel.anchoredPosition = hiddenPos;
        active = false;
        verticalTartgetPos = hiddenPos;
        targetPos = (float)currentPage / (totalPages - 1);
    }

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            active = !active;
            verticalTartgetPos = active ? shownPos : hiddenPos;
        }

        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, verticalTartgetPos, Time.deltaTime * transitionSpeed);

        if (!active) return;

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
