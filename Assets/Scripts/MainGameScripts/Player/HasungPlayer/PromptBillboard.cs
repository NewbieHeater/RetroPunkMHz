using UnityEngine;
using UnityEngine.UI;

public class PromptBillboard : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2.0f, 0);
    [SerializeField] private Camera cam;

    private Vector3 _worldPos;

    public void SetFollowTarget(Vector3 worldPos) => _worldPos = worldPos;
    public void SetText(string t) { if (label) label.text = t; }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;
        Vector3 screen = cam.WorldToScreenPoint(_worldPos + worldOffset);
        transform.position = screen;
    }
}
