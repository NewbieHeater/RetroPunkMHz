using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EtoRoominstall : MonoBehaviour
{
    private GameObject _objectPlace;
    private GameObject _previewobject;

    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        if (_previewobject == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스에 레이케스트 장착
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, groundLayer)) // 마우스가 땅에 있는 레이케스트랑 닿는다면
        {
            
            _previewobject.transform.position = hitInfo.point; // 땅하고 충돌지점으로 이동
            if (Input.GetMouseButtonDown(0)) 
            {
                PlaceObject(hitInfo.point); // 마우스 커서 위치에 오브젝트 설치
            }
        }
    }
    public void startPlacing(GameObject perfeb) // ui 클리식 오브젝트 받기
    {
        _objectPlace = perfeb;
        CreatePreview();
    }

    private void CreatePreview() 
    {
        if (_objectPlace == null) return;

        if (_previewobject != null)
        {
            Destroy(_previewobject);
        }

        _previewobject = Instantiate(_objectPlace); // 설치가 눈에 보이게 복사
        SetPreviewMaterial(_previewobject, 0.5f); // 투명도 낮추기

    }

    void PlaceObject(Vector3 position) 
    {
        Instantiate(_objectPlace, position, Quaternion.identity); // 인스턴스로 오브젝트 설치
        Destroy(_previewobject); // 설치하려고 보이게 한거 파괴
        _previewobject = null;
        _objectPlace = null;
    }

    void SetPreviewMaterial(GameObject gameObject, float alpha)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>(); // 오브젝트 렌더링 불러오기

        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials) // 렌더가 많을수도 있으니 2번 
            {
                Color c = mat.color; // c라는 변수에 오브젝트 컬러 복사
                c.a = alpha; // 투명도 조절
                mat.color = c; // 투명도 조절하고 렌더 색상에 넣기
            }
        }
    }
}
