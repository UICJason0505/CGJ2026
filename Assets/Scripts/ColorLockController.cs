using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ColorLockController : MonoBehaviour
{
    [Header("Color Lock UI")]
    [SerializeField] private GameObject colorLockUI;

    [Header("Color Sequence")]
    [SerializeField] private string[] colorSequence = { "Blue", "Green", "Red", "White", "Orange" };

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    [Header("Wrong Sequence Feedback")]
    [SerializeField] private float flashInterval = 0.15f;

    private readonly Dictionary<string, Button> colorButtons = new Dictionary<string, Button>();
    private readonly Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    private int progress;
    private bool isProcessing;

    private void Awake()
    {
        if (colorLockUI == null)
            colorLockUI = gameObject.name == "ColorLock" ? gameObject : GameObject.Find("ColorLock");

        HidePasswordLockIfPresent();
        CacheColorButtons();
    }

    private void Start()
    {
        RegisterColorButtons();
        RegisterEscButton();
    }

    private void CacheColorButtons()
    {
        colorButtons.Clear();
        originalColors.Clear();

        if (colorLockUI == null)
            return;

        foreach (var button in colorLockUI.GetComponentsInChildren<Button>(true))
        {
            if (!IsColorButtonName(button.gameObject.name))
                continue;

            colorButtons[button.gameObject.name] = button;

            var image = button.GetComponent<Image>();
            if (image != null)
                originalColors[button] = image.color;
        }
    }

    private bool IsColorButtonName(string buttonName)
    {
        foreach (var colorName in colorSequence)
        {
            if (buttonName == colorName)
                return true;
        }

        return false;
    }

    private void RegisterColorButtons()
    {
        foreach (var pair in colorButtons)
        {
            var colorName = pair.Key;
            pair.Value.onClick.AddListener(() => OnColorPressed(colorName));
        }
    }

    private void RegisterEscButton()
    {
        if (colorLockUI == null)
            return;

        foreach (var button in colorLockUI.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name != "Esc")
                continue;

            button.onClick.AddListener(HideColorLock);
        }
    }

    private void OnColorPressed(string colorName)
    {
        if (isProcessing || progress >= colorSequence.Length)
            return;

        if (colorName != colorSequence[progress])
        {
            StartCoroutine(WrongSequenceFeedback());
            return;
        }

        progress++;

        if (progress >= colorSequence.Length)
            StartCoroutine(CorrectSequence());
    }

    private IEnumerator CorrectSequence()
    {
        isProcessing = true;
        SetColorButtonsInteractable(false);

        if (colorLockUI != null)
            colorLockUI.SetActive(false);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);

        yield break;
    }

    private IEnumerator WrongSequenceFeedback()
    {
        isProcessing = true;
        SetColorButtonsInteractable(false);

        SetAllColorButtonsColor(Color.red);
        yield return new WaitForSeconds(flashInterval);

        RestoreColorButtonColors();
        yield return new WaitForSeconds(flashInterval);

        SetAllColorButtonsColor(Color.red);
        yield return new WaitForSeconds(flashInterval);

        RestoreColorButtonColors();

        progress = 0;
        isProcessing = false;
        SetColorButtonsInteractable(true);
    }

    private void SetAllColorButtonsColor(Color color)
    {
        foreach (var button in colorButtons.Values)
        {
            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = color;
        }
    }

    private void RestoreColorButtonColors()
    {
        foreach (var pair in originalColors)
        {
            var image = pair.Key.GetComponent<Image>();
            if (image != null)
                image.color = pair.Value;
        }
    }

    private void SetColorButtonsInteractable(bool interactable)
    {
        foreach (var button in colorButtons.Values)
            button.interactable = interactable;
    }

    public void ShowColorLock()
    {
        progress = 0;
        HidePasswordLockIfPresent();

        if (colorLockUI != null)
            colorLockUI.SetActive(true);
    }

    private static void HidePasswordLockIfPresent()
    {
        var passwordLock = GameObject.Find("PasswordLock");
        if (passwordLock != null)
            passwordLock.SetActive(false);
    }

    private void HideColorLock()
    {
        if (isProcessing)
            return;

        progress = 0;

        if (colorLockUI != null)
            colorLockUI.SetActive(false);
    }
}
