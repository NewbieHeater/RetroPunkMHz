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
    public Vector2 verticalTargetPos;

    private void Start()
    {
        panel.anchoredPosition = hiddenPos;
        active = false;
        verticalTargetPos = hiddenPos;
        targetPos = (float)currentPage / (totalPages - 1);
    }

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
            ToggleMenu();

        float dt = Time.unscaledDeltaTime;
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, verticalTargetPos, dt * transitionSpeed);

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

    void ToggleMenu()
    {
        active = !active;
        verticalTargetPos = active ? shownPos : hiddenPos;

        if (active)
        {
            GameManager.Instance.GamePause();
            AudioListener.pause = true;     // ���� ����: ����� �Ͻ�����
        }
        else
        {
            GameManager.Instance.GamePause();
            AudioListener.pause = false;
        }
    }
}
