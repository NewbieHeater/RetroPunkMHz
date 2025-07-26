using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// 인벤토리에서 아이템의 설명을 보도록 한다.
/// </summary>
public class ItemDescription : MonoBehaviour
{
    [Header("텍스트 관련 오브젝트")]
    [SerializeField] private GameObject mToolTipObj;
    [SerializeField] private Canvas mCanvas;

    [Header("매니저")]
    [SerializeField] private ItemDataManager mItemDataManager;

    private TextMeshProUGUI mTextArea; //텍스트 라벨
    private RectTransform mRectTransform; //UI 트랜스폼

    private StringBuilder mStringBuilder; //스트링 빌더

    private void Start()
    {
        mTextArea = mToolTipObj.GetComponentInChildren<TextMeshProUGUI>();
        mRectTransform = mCanvas.GetComponent<RectTransform>();

        mStringBuilder = new StringBuilder();

        mToolTipObj.SetActive(false);
    }

    public void LateUpdate()
    {
        //if (mToolTipObj.activeInHierarchy) CalcMousePosition();
    }

    /// <summary>
    /// InventorySlot에서 호출되며 아이템 정보를 보여준다.
    /// </summary>
    /// <param name="id"></param>
    public void OpenUI(string name, string Description)
    {
        mStringBuilder.Clear();

        //이름 가져오기
        mStringBuilder.Append("<b>");
        mStringBuilder.AppendLine(name);
        mStringBuilder.Append("</b>");

        //설명 가져오기
        mStringBuilder.AppendLine();
        mStringBuilder.AppendLine(Description);

        //텍스트 replace
        mTextArea.SetText(mStringBuilder.ToString());
        mToolTipObj.SetActive(true);
    }
    //public void OpenUI(int id)
    //{
    //    mStringBuilder.Clear();

    //    //이름 가져오기
    //    mStringBuilder.Append("<b>");
    //    mStringBuilder.AppendLine(mItemDataManager.GetName(id));
    //    mStringBuilder.Append("</b>");

    //    //설명 가져오기
    //    mStringBuilder.AppendLine();
    //    mStringBuilder.AppendLine(mItemDataManager.GetDescription(id));

    //    //텍스트 replace
    //    mTextArea.SetText(mStringBuilder.ToString());
    //    mToolTipObj.SetActive(true);
    //}
    /// <summary>
    /// 설명 UI를 닫는다.
    /// </summary>
    public void CloseUI()
    {
        mToolTipObj.SetActive(false);
    }

    /// <summary>
    /// 마우스의 위치를 계산하여 설명UI가 마우스를 따라다니게 한다.
    /// </summary>
    //private void CalcMousePosition()
    //{
    //    Vector2 localPosition; // 변환된 canvas내 현재 좌표
    //    Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); //마우스의 현재 위치

    //    // 텍스트 라벨을 가져온다.
    //    RectTransform rt = mToolTipObj.transform as RectTransform;

    //    // 마우스 좌표를 canvas내에서의 좌표로 변환
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, mousePosition, mCanvas.worldCamera, out localPosition);

    //    //툴팁이 마우스 기준 우측에 나오는것을 고려하여 마우스의 위치가 가로길이 기준 75%를 초과하면 마우스기준 우측으로 나타나도록 한다.
    //    //계산식은 현재 나타난 텍스트UI의 가로길이만큼을 빼주며, *0.5f는 Scale이 0.5f로 설정되어있기때문에 사용
    //    if (mousePosition.x > Screen.width * 0.75f) { localPosition.x -= rt.sizeDelta.x * 0.5f; }

    //    // 위치 변경
    //    rt.anchoredPosition = localPosition;
    //}
}