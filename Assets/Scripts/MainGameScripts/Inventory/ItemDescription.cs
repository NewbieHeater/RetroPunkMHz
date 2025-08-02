using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// �κ��丮���� �������� ������ ������ �Ѵ�.
/// </summary>
public class ItemDescription : MonoBehaviour
{
    [Header("�ؽ�Ʈ ���� ������Ʈ")]
    [SerializeField] private GameObject _toolTipObj;
    [SerializeField] private Canvas _canvas;


    private TextMeshProUGUI _textArea; //�ؽ�Ʈ ��
    private RectTransform _rectTransform; //UI Ʈ������

    private StringBuilder _stringBuilder; //��Ʈ�� ����

    private void Start()
    {
        _textArea = _toolTipObj.GetComponentInChildren<TextMeshProUGUI>();
        _rectTransform = _canvas.GetComponent<RectTransform>();

        _stringBuilder = new StringBuilder();

        _toolTipObj.SetActive(false);
    }

    public void LateUpdate()
    {
        //if (mToolTipObj.activeInHierarchy) CalcMousePosition();
    }

    /// <summary>
    /// InventorySlot���� ȣ��Ǹ� ������ ������ �����ش�.
    /// </summary>
    /// <param name="id"></param>
    public void OpenUI(string name, string Description)
    {
        _stringBuilder.Clear();

        //�̸� ��������
        _stringBuilder.Append("<b>");
        _stringBuilder.AppendLine(name);
        _stringBuilder.Append("</b>");

        //���� ��������
        _stringBuilder.AppendLine(Description);

        //�ؽ�Ʈ replace
        _textArea.SetText(_stringBuilder.ToString());
        _toolTipObj.SetActive(true);
    }
    //public void OpenUI(int id)
    //{
    //    mStringBuilder.Clear();

    //    //�̸� ��������
    //    mStringBuilder.Append("<b>");
    //    mStringBuilder.AppendLine(mItemDataManager.GetName(id));
    //    mStringBuilder.Append("</b>");

    //    //���� ��������
    //    mStringBuilder.AppendLine();
    //    mStringBuilder.AppendLine(mItemDataManager.GetDescription(id));

    //    //�ؽ�Ʈ replace
    //    mTextArea.SetText(mStringBuilder.ToString());
    //    mToolTipObj.SetActive(true);
    //}
    /// <summary>
    /// ���� UI�� �ݴ´�.
    /// </summary>
    public void CloseUI()
    {
        _toolTipObj.SetActive(false);
    }

    /// <summary>
    /// ���콺�� ��ġ�� ����Ͽ� ����UI�� ���콺�� ����ٴϰ� �Ѵ�.
    /// </summary>
    //private void CalcMousePosition()
    //{
    //    Vector2 localPosition; // ��ȯ�� canvas�� ���� ��ǥ
    //    Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); //���콺�� ���� ��ġ

    //    // �ؽ�Ʈ ���� �����´�.
    //    RectTransform rt = mToolTipObj.transform as RectTransform;

    //    // ���콺 ��ǥ�� canvas�������� ��ǥ�� ��ȯ
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, mousePosition, mCanvas.worldCamera, out localPosition);

    //    //������ ���콺 ���� ������ �����°��� ����Ͽ� ���콺�� ��ġ�� ���α��� ���� 75%�� �ʰ��ϸ� ���콺���� �������� ��Ÿ������ �Ѵ�.
    //    //������ ���� ��Ÿ�� �ؽ�ƮUI�� ���α��̸�ŭ�� ���ָ�, *0.5f�� Scale�� 0.5f�� �����Ǿ��ֱ⶧���� ���
    //    if (mousePosition.x > Screen.width * 0.75f) { localPosition.x -= rt.sizeDelta.x * 0.5f; }

    //    // ��ġ ����
    //    rt.anchoredPosition = localPosition;
    //}
}