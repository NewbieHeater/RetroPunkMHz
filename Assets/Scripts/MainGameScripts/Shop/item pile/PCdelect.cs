using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PCdelect : MonoBehaviour
{
    [SerializeField] private Item returnItem;
    private PC _pc;
    bool isMouseOver = false;
    
    void OnMouseEnter() => isMouseOver = true;
    void OnMouseExit() => isMouseOver = false;

    private GameObject _pcui;
    private Button _ampbutton;
    private Button _perbutton;
    private Button _wavbutton;
    public void SetReturnItem(Item item)
    {
        returnItem = item;

    }
    public void Start()
    {
        gameObject.SetActive(true);
        _pc = GetComponent<PC>();
        if (_pcui == null)
        {
            Transform canvas = GameObject.Find("Canvas").transform;
            _pcui = canvas.Find("PCUI").gameObject;
        }
        Transform image = _pcui.transform.Find("Image");
        if (image == null)
        {
            Debug.LogError("PCUI 안에 'Image' 오브젝트가 없습니다!");
            return;
        }
        _ampbutton = image.Find("amplitudePts").GetComponent<Button>();
        _perbutton = image.Find("periodPts").GetComponent<Button>();
        _wavbutton = image.Find("waveformPts").GetComponent<Button>();
        _ampbutton.onClick.AddListener(amp_multiply);
        _perbutton.onClick.AddListener(per_multiply);
        _wavbutton.onClick.AddListener(wav_multiply);
    }
    private void Update()
    {
        if (isMouseOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                itmedelect();
            }

            if (Input.GetMouseButtonDown(1))
            {
                toogle_openUi();
            }
        }

    }
    void itmedelect()
    {

        InventoryMain.Instance.AcquireItem(returnItem);
        Destroy(gameObject);
    }

    void toogle_openUi()
    {
        
         _pcui.SetActive(true);
         
        
    }

    public void amp_multiply()
    {
        Debug.Log("이이");
        StartCoroutine(CloseAndExecute(() => _pc.pc_amp_multiply()));

    }
    public void per_multiply()
    {
        Debug.Log("이이");
        StartCoroutine(CloseAndExecute(() => _pc.pc_per_multiply()));

    }
    public void wav_multiply()
    {
        Debug.Log("이이");
        StartCoroutine(CloseAndExecute(() => _pc.pc_wav_multiply()));

    }
    IEnumerator CloseAndExecute(System.Action action)
    {
        yield return null;
        _pcui.SetActive(false);
        action.Invoke();
    }
}
