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
    public Vector2 hiddenPos;  // 숨겨진 위치 (위로)
    public Vector2 shownPos;     // 보이는 위치
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

        // 좌우 키 입력
        if (Input.GetKeyDown(KeyCode.Q)) SlideToPage(currentPage - 1);
        if (Input.GetKeyDown(KeyCode.E)) SlideToPage(currentPage + 1);

        // 부드럽게 이동
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
        targetPos = (float)currentPage / (totalPages - 1); // 0.0 ~ 1.0 사이 값
    }
}
