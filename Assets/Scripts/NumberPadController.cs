using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NumberPadController : MonoBehaviour
{
    [Header("Password Lock UI")]
    [SerializeField] private GameObject passwordLockUI;
    [SerializeField] private TMP_Text answerText;

    [Header("Password")]
    [SerializeField] private string correctPassword = "317";

    [Header("Unlock Result")]
    [SerializeField] private GameObject objectToShowAfterUnlock;
    [SerializeField] private GameObject objectToHideAfterUnlock;
    [SerializeField] private UnityEvent onUnlocked;

    [Header("Storyline")]
    [SerializeField] private StorylineController storylineController;
    [SerializeField] private int targetSequence = 30;

    [Header("Wrong Password Feedback")]
    [SerializeField] private float flashInterval = 0.15f;

    private string currentInput = string.Empty;
    private Color originalTextColor = Color.black;
    private bool isProcessing;
    private bool unlocked;

    private void Awake()
    {
        if (passwordLockUI == null)
        {
            passwordLockUI = gameObject;
        }

        if (answerText == null)
        {
            answerText = FindAnswerText();
        }

        if (answerText != null)
        {
            originalTextColor = answerText.color;
            answerText.text = string.Empty;
        }
    }

    private void Start()
    {
        RegisterEscButton();
    }

    private TMP_Text FindAnswerText()
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name == "Answer" || text.gameObject.name == "Text (TMP)")
            {
                return text;
            }
        }

        return null;
    }

    private void RegisterNumberButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            int digit = GetDigitFromButtonName(button.gameObject.name);

            if (digit < 0)
            {
                continue;
            }

            button.onClick.AddListener(() => PressDigit(digit));
        }
    }

    private int GetDigitFromButtonName(string buttonName)
    {
        if (!buttonName.StartsWith("Number"))
        {
            return -1;
        }

        string digitText = buttonName.Substring("Number".Length);

        if (int.TryParse(digitText, out int digit))
        {
            return digit;
        }

        return -1;
    }

    private void RegisterEscButton()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.gameObject.name == "Esc")
            {
                button.onClick.AddListener(HidePasswordLock);
            }
        }
    }

    public void Press1() => PressDigit(1);
    public void Press2() => PressDigit(2);
    public void Press3() => PressDigit(3);
    public void Press4() => PressDigit(4);
    public void Press5() => PressDigit(5);
    public void Press6() => PressDigit(6);
    public void Press7() => PressDigit(7);
    public void Press8() => PressDigit(8);
    public void Press9() => PressDigit(9);

    public void PressDigit(int digit)
    {
        if (isProcessing || unlocked)
        {
            return;
        }

        if (answerText == null)
        {
            answerText = FindAnswerText();
        }

        if (answerText == null)
        {
            Debug.LogWarning("[NumberPad] Answer Text is missing.");
            return;
        }

        if (currentInput.Length >= correctPassword.Length)
        {
            return;
        }

        currentInput += digit.ToString();
        RefreshAnswerText();

        if (currentInput.Length >= correctPassword.Length)
        {
            CheckPassword();
        }
    }

    private void RefreshAnswerText()
    {
        answerText.text = string.Join(" ", currentInput.ToCharArray());
    }

    private void CheckPassword()
    {
        if (currentInput == correctPassword)
        {
            StartCoroutine(Unlock());
        }
        else
        {
            StartCoroutine(WrongPasswordFeedback());
        }
    }

    private IEnumerator Unlock()
    {
        isProcessing = true;
        unlocked = true;
        SetNumberButtonsInteractable(false);

        if (objectToShowAfterUnlock != null)
        {
            objectToShowAfterUnlock.SetActive(true);
        }

        if (objectToHideAfterUnlock != null)
        {
            objectToHideAfterUnlock.SetActive(false);
        }

        onUnlocked?.Invoke();

        if (passwordLockUI != null)
        {
            passwordLockUI.SetActive(false);
        }

        yield break;
    }

    private IEnumerator WrongPasswordFeedback()
    {
        isProcessing = true;
        SetNumberButtonsInteractable(false);

        Vector3 originalPosition = answerText.transform.localPosition;

        answerText.color = Color.red;
        answerText.transform.localPosition = originalPosition + new Vector3(-8f, 0f, 0f);
        yield return new WaitForSeconds(flashInterval);

        answerText.transform.localPosition = originalPosition + new Vector3(8f, 0f, 0f);
        yield return new WaitForSeconds(flashInterval);

        answerText.transform.localPosition = originalPosition;
        yield return new WaitForSeconds(flashInterval);

        currentInput = string.Empty;
        answerText.text = string.Empty;
        answerText.color = originalTextColor;
        answerText.transform.localPosition = originalPosition;

        isProcessing = false;
        SetNumberButtonsInteractable(true);
    }

    private void SetNumberButtonsInteractable(bool interactable)
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.gameObject.name.StartsWith("Number"))
            {
                button.interactable = interactable;
            }
        }
    }

    public void ShowPasswordLock()
    {
        if (unlocked)
        {
            return;
        }

        currentInput = string.Empty;

        if (answerText != null)
        {
            answerText.text = string.Empty;
            answerText.color = originalTextColor;
        }

        if (passwordLockUI != null)
        {
            passwordLockUI.SetActive(true);
        }
    }

    public void AdvanceStoryline()
    {
        if (storylineController != null)
            storylineController.GoToSequence(targetSequence);
    }

    public void HidePasswordLock()
    {
        if (isProcessing)
        {
            return;
        }

        currentInput = string.Empty;

        if (answerText != null)
        {
            answerText.text = string.Empty;
            answerText.color = originalTextColor;
        }

        if (passwordLockUI != null)
        {
            passwordLockUI.SetActive(false);
        }
    }
}