using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全屏过渡遮罩 + 圆形进度条。由 MenuManager 在运行时创建到主 Canvas 下。
/// PlayLoading 显示遮罩、填充圆形进度条，结束后自动隐藏。
/// </summary>
public class ScreenTransition : MonoBehaviour
{
    private Image fillImage;
    private CanvasGroup overlayGroup;

    public static ScreenTransition Create(Canvas canvas)
    {
        var go = new GameObject("ScreenTransition");
        go.transform.SetParent(canvas.transform, false);
        var st = go.AddComponent<ScreenTransition>();
        st.Build();
        return st;
    }

    private void Build()
    {
        var circle = CreateCircleSprite();

        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(transform, false);
        var overlayRect = overlayGo.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        var overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.55f);
        overlayImg.raycastTarget = true;
        overlayGroup = overlayGo.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;
        overlayGo.transform.SetAsLastSibling();

        var trackGo = new GameObject("Track");
        trackGo.transform.SetParent(overlayGo.transform, false);
        var trackRect = trackGo.AddComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.5f, 0.5f);
        trackRect.anchorMax = new Vector2(0.5f, 0.5f);
        trackRect.pivot = new Vector2(0.5f, 0.5f);
        trackRect.sizeDelta = new Vector2(110f, 110f);
        var trackImg = trackGo.AddComponent<Image>();
        trackImg.sprite = circle;
        trackImg.color = new Color(1f, 1f, 1f, 0.25f);
        trackImg.raycastTarget = false;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(overlayGo.transform, false);
        var fillRect = fillGo.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.5f, 0.5f);
        fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.sizeDelta = new Vector2(110f, 110f);
        fillImage = fillGo.AddComponent<Image>();
        fillImage.sprite = circle;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillAmount = 0f;
        fillImage.color = Color.white;
        fillImage.raycastTarget = false;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                tex.SetPixel(x, y, dist <= radius ? Color.white : new Color(0f, 0f, 0f, 0f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public IEnumerator PlayLoading(float duration)
    {
        overlayGroup.alpha = 1f;
        overlayGroup.blocksRaycasts = true;
        fillImage.fillAmount = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fillImage.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fillImage.fillAmount = 1f;
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;
    }
}
