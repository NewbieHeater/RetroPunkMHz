using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonHovering : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler
{
    Vector3 originalScale;
    [SerializeField] bool mindSelected;
    bool isSelected;
    [SerializeField] Vector3 targetScale;
    [SerializeField] float time;
    [SerializeField] Canvas canvas;
    Animator canvasAnim;
    public UnityEvent OnClick;
    public UnityEvent OnDeselect;

    RectTransform rect;
    private void Start()
    {
        canvasAnim = canvas.GetComponent<Animator>();
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(ScaleCoroutine(targetScale, time));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected && mindSelected) return; 
        StartCoroutine(ScaleCoroutine(originalScale, time));

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TitleManager.Instance.selectedButton = this;
        OnClick?.Invoke();
        isSelected = true;
    }
    public void Deselect()
    {
        isSelected = false;
        OnDeselect?.Invoke();
        StartCoroutine(ScaleCoroutine(originalScale, time));
    }
    public void NewGame(bool flag)
    {
        TitleManager.Instance.CamNewGame(flag);
        canvasAnim.SetBool("NewGame", flag);
    }

    private IEnumerator ScaleCoroutine(Vector3 scale, float duration)
    {
        Vector3 startScale = rect.localScale;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rect.localScale = Vector3.Lerp(startScale, scale, t);
            yield return null;
        }

        rect.localScale = scale; // 마지막 보정
    }

}
