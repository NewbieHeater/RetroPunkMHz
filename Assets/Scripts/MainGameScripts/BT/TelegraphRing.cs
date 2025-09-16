using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TelegraphRing : MonoBehaviour
{
    [SerializeField] float duration = 0.8f;
    [SerializeField] float width = 1f;
    [SerializeField] Color color = new Color(1, 0, 0, 0.7f);

    LineRenderer lr;
    float t;

    // 재사용 가능한 공유 머티리얼(인스턴스 누수 방지)
    static Material sSharedMat;

    public void Init(Vector3 start, Vector3 end, float duration, float width, Color color)
    {
        this.duration = duration;
        this.width = width;
        this.color = color;

        if (!lr) lr = GetComponent<LineRenderer>();

        // 머티리얼 1회만 생성해 공유
        if (sSharedMat == null)
        {
            var shader = Shader.Find("Sprites/Default"); // 알파 페이드 간단
            sSharedMat = new Material(shader);
            sSharedMat.renderQueue = 3000; // Transparent
        }

        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lr.endWidth = width;
        lr.material = sSharedMat;            // 공유 머티리얼 사용
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCapVertices = 4;               // 끝단 살짝 둥글게
        lr.startColor = lr.endColor = color;
    }

    void Awake() { lr = GetComponent<LineRenderer>(); }

    void Update()
    {
        t += Time.deltaTime;
        float u = Mathf.Clamp01(t / duration);

        // 알파 페이드아웃
        var c = color;
        c.a = Mathf.Lerp(color.a, 0f, u);
        lr.startColor = lr.endColor = c;

        // (선택) 살짝 깜빡이는 효과를 주고 싶다면,
        // float pulse = 0.5f + 0.5f * Mathf.Sin(t * 18f);
        // lr.startWidth = lr.endWidth = Mathf.Lerp(width * 0.8f, width * 1.1f, pulse);

        if (t >= duration) Destroy(gameObject);
    }
}
