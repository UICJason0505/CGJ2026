using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFadeIn : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void Show(int index)
    {
        if (index < 0 || index >= buttons.Length) return;
        var btn = buttons[index];
        if (btn == null) return;

        btn.gameObject.SetActive(true);
        var cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        StartCoroutine(FadeOne(cg));
    }

    public void Show0() => Show(0);
    public void Show1() => Show(1);
    public void Show2() => Show(2);
    public void Show3() => Show(3);
    public void Show4() => Show(4);

    private IEnumerator FadeOne(CanvasGroup cg)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = fadeCurve.Evaluate(elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}
