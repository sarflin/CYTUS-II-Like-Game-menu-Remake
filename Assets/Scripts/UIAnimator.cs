using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private bool isAnimating = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void FadeIn()
    {
        if (!isAnimating)
        {
            StartCoroutine(Fade(0f, 1f, fadeInDuration));
        }
    }

    public void FadeOut()
    {
        if (!isAnimating)
        {
            StartCoroutine(Fade(1f, 0f, fadeOutDuration));
        }
    }

    private System.Collections.IEnumerator Fade(float from, float to, float duration)
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
        isAnimating = false;
    }

    public void ScaleIn()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(Scale(Vector3.zero, Vector3.one, fadeInDuration));
    }

    private System.Collections.IEnumerator Scale(Vector3 from, Vector3 to, float duration)
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localScale = to;
        isAnimating = false;
    }
}
