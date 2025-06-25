using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tabMenu : MonoBehaviour
{
    public KeyCode activationKey;
    public bool active;
    public RectTransform panel;
    public float transitionSpeed;

    public Vector2 hiddenPos;  // ������ ��ġ (����)
    public Vector2 shownPos;     // ���̴� ��ġ
    public Vector2 tartgetPos;


    // Start is called before the first frame update
    void Start()
    {
        tartgetPos = hiddenPos;
        active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(activationKey))
            ToggleMenu();

        float dt = Time.unscaledDeltaTime;
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, tartgetPos, dt * transitionSpeed);
    }

    void ToggleMenu()
    {
        active = !active;
        tartgetPos = active ? shownPos : hiddenPos;

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
