using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorLockController : MonoBehaviour
{
    [Header("Color Lock UI")]
    [SerializeField] private GameObject colorLockUI;

    [Header("Password")]
    [SerializeField] private string[] password = { "Blue", "Green", "Red", "White", "Orange" };

    [Header("Unlock Result")]
    [SerializeField] private GameObject objectToShowAfterUnlock;
    [SerializeField] private GameObject objectToHideAfterUnlock;
    [SerializeField] private UnityEvent onUnlocked;

    [Header("Wrong Password Feedback")]
    [SerializeField] private float flashInterval = 0.15f;

    private readonly Dictionary<string, Button> colorButtons = new Dictionary<string, Button>();
    private readonly Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    private int progress;
    private bool isProcessing;
    private bool unlocked;

    private void Awake()
    {
        if (colorLockUI == null)
        {
            colorLockUI = gameObject;
        }

        CacheColorButtons();
    }

    private void Start()
    {
        RegisterColorButtons();
        RegisterEscButton();
        ResetLock();
    }

    private void CacheColorButtons()
    {
        colorButtons.Clear();
        originalColors.Clear();

        if (colorLockUI == null)
        {
            return;
        }

        foreach (Button button in colorLockUI.GetComponentsInChildren<Button>(true))
        {
            string buttonName = button.gameObject.name;

            if (!IsPasswordColor(buttonName))
            {
                continue;
            }

            colorButtons[buttonName] = button;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                originalColors[button] = image.color;
            }
        }
    }

    private bool IsPasswordColor(string colorName)
    {
        for (int i = 0; i < password.Length; i++)
        {
            if (password[i] == colorName)
            {
                return true;
            }
        }

        return false;
    }

    private void RegisterColorButtons()
    {
        foreach (KeyValuePair<string, Button> pair in colorButtons)
        {
            string colorName = pair.Key;
            pair.Value.onClick.AddListener(() => PressColor(colorName));
        }
    }

    private void RegisterEscButton()
    {
        if (colorLockUI == null)
        {
            return;
        }

        foreach (Button button in colorLockUI.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name == "Esc")
            {
                button.onClick.AddListener(HideColorLock);
            }
        }
    }

    public void PressBlue()
    {
        PressColor("Blue");
    }

    public void PressGreen()
    {
        PressColor("Green");
    }

    public void PressRed()
    {
        PressColor("Red");
    }

    public void PressWhite()
    {
        PressColor("White");
    }

    public void PressOrange()
    {
        PressColor("Orange");
    }

    public void PressColor(string colorName)
    {
        if (isProcessing || unlocked || progress >= password.Length)
        {
            return;
        }

        if (colorName != password[progress])
        {
            StartCoroutine(WrongPasswordFeedback());
            return;
        }

        progress++;

        if (progress >= password.Length)
        {
            StartCoroutine(Unlock());
        }
    }

    private IEnumerator Unlock()
    {
        isProcessing = true;
        unlocked = true;
        SetColorButtonsInteractable(false);

        if (objectToShowAfterUnlock != null)
        {
            objectToShowAfterUnlock.SetActive(true);
        }

        if (objectToHideAfterUnlock != null)
        {
            objectToHideAfterUnlock.SetActive(false);
        }

        onUnlocked?.Invoke();

        if (colorLockUI != null)
        {
            colorLockUI.SetActive(false);
        }

        yield break;
    }

    private IEnumerator WrongPasswordFeedback()
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
        foreach (Button button in colorButtons.Values)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }
    }

    private void RestoreColorButtonColors()
    {
        foreach (KeyValuePair<Button, Color> pair in originalColors)
        {
            Image image = pair.Key.GetComponent<Image>();
            if (image != null)
            {
                image.color = pair.Value;
            }
        }
    }

    private void SetColorButtonsInteractable(bool interactable)
    {
        foreach (Button button in colorButtons.Values)
        {
            button.interactable = interactable;
        }
    }

    public void ShowColorLock()
    {
        if (unlocked)
        {
            return;
        }

        ResetLock();

        if (colorLockUI != null)
        {
            colorLockUI.SetActive(true);
        }
    }

    public void HideColorLock()
    {
        if (isProcessing)
        {
            return;
        }

        ResetLock();

        if (colorLockUI != null)
        {
            colorLockUI.SetActive(false);
        }
    }

    public void ResetLock()
    {
        progress = 0;
        isProcessing = false;
        RestoreColorButtonColors();
        SetColorButtonsInteractable(true);
    }
}