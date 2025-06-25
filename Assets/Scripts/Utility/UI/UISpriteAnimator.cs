using UnityEngine;
using UnityEngine.UI;

public class UISpriteSheetAnimator : MonoBehaviour
{
    public Image targetImage;         // UI 이미지
    public Sprite[] sprites;          // 시트에서 잘라낸 프레임들
    public float frameRate = 24f;     // 초당 프레임

    private int currentFrame = 0;
    private float timer = 0f;

    void Update()
    {
        if (sprites == null || sprites.Length == 0 || targetImage == null)
            return;

        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame = (currentFrame + 1) % sprites.Length;
            targetImage.sprite = sprites[currentFrame];
        }
    }
}
