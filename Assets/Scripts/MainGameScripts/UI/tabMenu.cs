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

    public Vector2 hiddenPos;  // 숨겨진 위치 (위로)
    public Vector2 shownPos;     // 보이는 위치
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
        {
            active = !active;
            tartgetPos = active ? shownPos : hiddenPos;
        }

        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, tartgetPos, Time.deltaTime * transitionSpeed);
    }
}
