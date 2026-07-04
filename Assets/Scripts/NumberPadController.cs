using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NumberPadController : MonoBehaviour
{
    [Header("Answer Display")]
    [SerializeField] private TMP_Text answerText;

    [Header("Password")]
    [SerializeField] private string correctPassword = "1234";

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject uiRoot;

    [Header("Password Lock UI")]
    [SerializeField] private GameObject passwordLockUI;
    [SerializeField] private Button lockButton;

    [Header("Wrong Password Feedback")]
    [SerializeField] private float flashInterval = 0.15f;

    private const int MaxLength = 4;

    private string currentInput = string.Empty;
    private Color originalTextColor;
    private bool isProcessing;

    private void Awake()
    {
        EnsurePasswordLockReference();

        if (answerText == null)
            answerText = FindAnswerText();

        if (uiRoot == null)
            uiRoot = passwordLockUI;

        if (passwordLockUI != null)
            passwordLockUI.SetActive(false);

        if (answerText != null)
            originalTextColor = answerText.color;
    }

    private void EnsurePasswordLockReference()
    {
        if (passwordLockUI == null)
        {
            passwordLockUI = GameObject.Find("PasswordLock");
            return;
        }

        if (IsLockObject(passwordLockUI))
        {
            Debug.LogWarning("[NumberPadController] Password Lock UI 应拖入 PasswordLock 面板，不要拖 Lock 按钮。已尝试自动查找 PasswordLock。");
            passwordLockUI = GameObject.Find("PasswordLock");
        }
    }

    private TMP_Text FindAnswerText()
    {
        if (passwordLockUI != null)
        {
            foreach (var text in passwordLockUI.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.gameObject.name == "Answer")
                    return text;
            }
        }

        var answerObject = GameObject.Find("Answer");
        return answerObject != null ? answerObject.GetComponent<TMP_Text>() : null;
    }

    private void Start()
    {
        RegisterNumberButtons();
        RegisterLockButton();
        RegisterEscButton();
    }

    private void RegisterNumberButtons()
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var button in buttons)
        {
            if (!button.gameObject.name.StartsWith("Number"))
                continue;

            var capturedButton = button;
            capturedButton.onClick.AddListener(() => AppendDigitFromButton(capturedButton));
        }
    }

    private void RegisterLockButton()
    {
        var lockObject = GetLockObject();
        if (lockObject == null)
        {
            Debug.LogError("[NumberPadController] 未找到 Lock。请确认场景中有 Tag 为 Lock 的物体。");
            return;
        }

        if (passwordLockUI == null)
        {
            Debug.LogError("[NumberPadController] 未找到 PasswordLock 面板。请在 Password Lock UI 字段拖入 PasswordLock。");
            return;
        }

        if (lockObject.GetComponent<RectTransform>() != null)
        {
            lockButton = lockObject.GetComponent<Button>();
            if (lockButton != null)
                lockButton.onClick.AddListener(ShowPasswordLock);
            return;
        }

        RegisterWorldLockClick(lockObject);
    }

    private GameObject GetLockObject()
    {
        if (lockButton != null)
            return lockButton.gameObject;

        var uiLockButton = FindUiLockButton();
        if (uiLockButton != null)
            return uiLockButton.gameObject;

        return GameObject.FindGameObjectWithTag("Lock");
    }

    private static Button FindUiLockButton()
    {
        var buttons = Object.FindObjectsOfType<Button>(true);
        foreach (var button in buttons)
        {
            if (IsLockObject(button.gameObject) && button.GetComponent<RectTransform>() != null)
                return button;
        }

        return null;
    }

    private void RegisterWorldLockClick(GameObject lockObject)
    {
        EnsureCollider2D(lockObject);

        var handler = lockObject.GetComponent<LockWorldClickHandler>();
        if (handler == null)
            handler = lockObject.AddComponent<LockWorldClickHandler>();

        handler.Initialize(this);
    }

    private static void EnsureCollider2D(GameObject lockObject)
    {
        if (lockObject.GetComponent<Collider2D>() != null)
            return;

        var spriteRenderer = lockObject.GetComponent<SpriteRenderer>();
        var collider = lockObject.AddComponent<BoxCollider2D>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
            collider.size = spriteRenderer.sprite.bounds.size;
    }

    private static bool IsLockObject(GameObject gameObject)
    {
        if (gameObject.name == "PasswordLock")
            return false;

        return gameObject.CompareTag("Lock")
            || gameObject.name == "Lock"
            || gameObject.name == "LockButton";
    }

    public void ShowPasswordLock()
    {
        if (passwordLockUI == null)
        {
            Debug.LogError("[NumberPadController] PasswordLock 未设置，无法显示密码面板。");
            return;
        }

        passwordLockUI.SetActive(true);
    }

    private void RegisterEscButton()
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var button in buttons)
        {
            if (button.gameObject.name != "Esc")
                continue;

            button.onClick.AddListener(HidePasswordLock);
        }
    }

    private void HidePasswordLock()
    {
        if (isProcessing)
            return;

        if (passwordLockUI != null)
            passwordLockUI.SetActive(false);

        currentInput = string.Empty;

        if (answerText != null)
        {
            answerText.text = string.Empty;
            answerText.color = originalTextColor;
        }
    }

    private void AppendDigitFromButton(Button button)
    {
        if (isProcessing || answerText == null || currentInput.Length >= MaxLength)
            return;

        var tmpText = button.GetComponentInChildren<TMP_Text>();
        if (tmpText != null && !string.IsNullOrEmpty(tmpText.text))
        {
            AppendDigit(tmpText.text);
            return;
        }

        var legacyText = button.GetComponentInChildren<Text>();
        if (legacyText != null && !string.IsNullOrEmpty(legacyText.text))
            AppendDigit(legacyText.text);
    }

    private void AppendDigit(string digit)
    {
        currentInput += digit;
        answerText.text = currentInput;

        if (currentInput.Length >= MaxLength)
            CheckPassword();
    }

    private void CheckPassword()
    {
        if (currentInput == correctPassword)
            StartCoroutine(CorrectPasswordSequence());
        else
            StartCoroutine(WrongPasswordFeedback());
    }

    private IEnumerator CorrectPasswordSequence()
    {
        isProcessing = true;
        SetButtonsInteractable(false);

        if (uiRoot != null)
            uiRoot.SetActive(false);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);

        yield break;
    }

    private IEnumerator WrongPasswordFeedback()
    {
        isProcessing = true;
        SetButtonsInteractable(false);

        var redColor = Color.red;
        answerText.color = redColor;
        yield return new WaitForSeconds(flashInterval);

        answerText.color = originalTextColor;
        yield return new WaitForSeconds(flashInterval);

        answerText.color = redColor;
        yield return new WaitForSeconds(flashInterval);

        currentInput = string.Empty;
        answerText.text = string.Empty;
        answerText.color = originalTextColor;

        isProcessing = false;
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var button in buttons)
        {
            if (button.gameObject.name.StartsWith("Number"))
                button.interactable = interactable;
        }
    }
}
