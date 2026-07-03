using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void Show(string speaker, string text, Sprite portrait)
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

        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }
}
