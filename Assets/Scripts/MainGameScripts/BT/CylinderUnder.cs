using System.Collections;
using UnityEngine;

public class CylinderUnder : MonoBehaviour
{
    [SerializeField] private Collider _col;
    [SerializeField] private float _speed = 3f;

    private Coroutine _fallRoutine;
    private bool _falling;

    // 노드에서 속도/지연을 주입
    public void Init(float speed)
    {
        _speed = speed;
    }

    public void EnableFall(float delay = 1f)
    {
        if (_fallRoutine == null)
            _fallRoutine = StartCoroutine(MoveToDown(delay));
    }

    private IEnumerator MoveToDown(float delay)
    {
        yield return new WaitForSeconds(delay);
        _falling = true;
        while (_falling)
        {
            transform.position -= transform.up * _speed * Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowHp.Instance.MinusTMP(100);
        }
        if (other.CompareTag("Ground"))
        {
            _falling = false; // 루프 종료
            if (_fallRoutine != null) { StopCoroutine(_fallRoutine); _fallRoutine = null; }
            Destroy(gameObject, 2f);
        }
    }
}
