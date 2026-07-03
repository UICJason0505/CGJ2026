using System.Collections;
using UnityEngine;

public class Dream : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryEventListener eventListener;

    [Header("Buttons")]
    [SerializeField] private GameObject button1;
    [SerializeField] private GameObject button2;
    [SerializeField] private GameObject button3;

    [Header("Story Events")]
    [SerializeField] private StoryEventSO onButton1Shown;
    [SerializeField] private StoryEventSO onButton2Shown;
    [SerializeField] private StoryEventSO onButton3Shown;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup _canvasGroup1;
    private CanvasGroup _canvasGroup2;
    private CanvasGroup _canvasGroup3;

    private Coroutine _transition;
    private int _currentStep;

    private void Start()
    {
        InitButton(button1, out _canvasGroup1);
        InitButton(button2, out _canvasGroup2);
        InitButton(button3, out _canvasGroup3);

        _currentStep = 0;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.W)) return;
        if (eventListener != null && eventListener.IsDescriptionActive) return;

        _currentStep++;
        switch (_currentStep)
        {
            case 1: ShowButton(1); break;
            case 2: ShowButton(2); break;
            case 3: ShowButton(3); break;
        }
    }

    private void ShowButton(int index)
    {
        if (_transition != null) StopCoroutine(_transition);
        _transition = StartCoroutine(Transition(index));
    }

    private IEnumerator Transition(int targetIndex)
    {
        // 隐藏上一个按钮
        switch (targetIndex - 1)
        {
            case 1:
                yield return HideButton(button1, _canvasGroup1);
                break;
            case 2:
                yield return HideButton(button2, _canvasGroup2);
                break;
        }

        // 显示目标按钮
        switch (targetIndex)
        {
            case 1:
                onButton1Shown?.Raise();
                yield return ShowButtonAnim(button1, _canvasGroup1);
                break;
            case 2:
                onButton2Shown?.Raise();
                yield return ShowButtonAnim(button2, _canvasGroup2);
                break;
            case 3:
                onButton3Shown?.Raise();
                yield return ShowButtonAnim(button3, _canvasGroup3);
                break;
        }
    }

    private IEnumerator HideButton(GameObject btn, CanvasGroup cg)
    {
        if (btn == null) yield break;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private IEnumerator ShowButtonAnim(GameObject btn, CanvasGroup cg)
    {
        if (btn == null) yield break;

        btn.transform.localScale = Vector3.zero;
        cg.alpha = 0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);
            btn.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        btn.transform.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    public void OnEvent1() => ShowButton(1);
    public void OnEvent2() => ShowButton(2);
    public void OnEvent3() => ShowButton(3);

    private void InitButton(GameObject btn, out CanvasGroup cg)
    {
        cg = null;
        if (btn == null) return;

        cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.AddComponent<CanvasGroup>();

        btn.transform.localScale = Vector3.zero;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
