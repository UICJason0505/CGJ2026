using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typewriter")]
    [SerializeField] private float charsPerSecond = 30f;

    private Coroutine _typingRoutine;
    private string _fullText;
    private System.Action _onComplete;

    public bool IsTyping => _typingRoutine != null;

    public void Show(string speaker, string text, Sprite portrait, bool useTypewriter = true, System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        if (speakerPortrait != null)
        {
            speakerPortrait.sprite = portrait;
            speakerPortrait.enabled = portrait != null;
        }

        if (speakerNameText != null)
            speakerNameText.text = speaker;

        _onComplete = onComplete;
        _fullText = text;

        if (_typingRoutine != null) StopCoroutine(_typingRoutine);

        if (useTypewriter)
        {
            _typingRoutine = StartCoroutine(TypeText());
        }
        else
        {
            dialogueText.text = text;
            _typingRoutine = null;
            _onComplete?.Invoke();
        }
    }

    public void Complete()
    {
        if (_typingRoutine == null) return;
        StopCoroutine(_typingRoutine);
        _typingRoutine = null;
        dialogueText.text = _fullText;
        _onComplete?.Invoke();
    }

    private IEnumerator TypeText()
    {
        dialogueText.text = "";
        for (int i = 0; i < _fullText.Length; i++)
        {
            dialogueText.text += _fullText[i];
            yield return new WaitForSeconds(1f / charsPerSecond);
        }
        _typingRoutine = null;
        _onComplete?.Invoke();
    }

    public void Hide()
    {
        Complete();
        gameObject.SetActive(false);
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }
}
