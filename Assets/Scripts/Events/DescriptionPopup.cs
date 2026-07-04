using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DescriptionPopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image popupImage;
    [SerializeField] private TextMeshProUGUI popupText;

    [Header("Fade In")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private System.Action _onClose;

    public void Init(Sprite sprite, string text, System.Action onClose = null)
    {
        _onClose = onClose;

        if (popupImage != null)
        {
            popupImage.sprite = sprite;
            popupImage.enabled = sprite != null;
        }

        if (popupText != null)
            popupText.text = text;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = fadeCurve.Evaluate(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - fadeCurve.Evaluate(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        _onClose?.Invoke();
        Destroy(gameObject);
    }
}
