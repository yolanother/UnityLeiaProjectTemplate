using System.Collections;
using UnityEngine;

public class CanvasGroupFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool isFadedOut = true;

    void Start()
    {
        canvasGroup.alpha = isFadedOut ? 0f : 1f;
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
        isFadedOut = false;
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(canvasGroup, canvasGroup.alpha, 0f, fadeDuration));
        isFadedOut = true;
    }

    public void Toggle()
    {
        if (isFadedOut)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    private IEnumerator Fade(CanvasGroup targetCanvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / duration);
            targetCanvasGroup.alpha = alpha;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        targetCanvasGroup.alpha = endAlpha;
    }
}