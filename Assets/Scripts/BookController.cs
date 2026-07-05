using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BookPage
{
    public Sprite image;
    [TextArea(3, 10)]
    public string text;
}

public class BookController : MonoBehaviour
{
    [SerializeField] private BookPage[] pages;

    [Header("Display")]
    [SerializeField] private Image pageImage;
    [SerializeField] private TextMeshProUGUI pageText;
    [SerializeField] private int startPage;

    [Header("Read")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject readButton;

    public int CurrentPage { get; private set; }
    public int PageCount => pages.Length;

    private void Start()
    {
        CurrentPage = Mathf.Clamp(startPage, 0, pages.Length - 1);
        ShowPage(CurrentPage);
        if (readButton != null) readButton.SetActive(false);
    }

    public void NextPage()
    {
        if (CurrentPage >= pages.Length - 1) return;
        CurrentPage++;
        ShowPage(CurrentPage);
    }

    public void PreviousPage()
    {
        if (CurrentPage <= 0) return;
        CurrentPage--;
        ShowPage(CurrentPage);
    }

    public void GoToPage(int index)
    {
        if (index < 0 || index >= pages.Length) return;
        CurrentPage = index;
        ShowPage(CurrentPage);
    }

    public void MarkAsRead()
    {
        if (animator == null) return;
        animator.SetBool("read", true);
        animator.SetBool("stop", false);
        if (readButton != null) readButton.SetActive(false);
    }

    public void StopReading()
    {
        if (animator == null) return;
        animator.SetBool("stop", true);
        animator.SetBool("read", false);
        StartCoroutine(ShowReadButtonAfterDelay());
    }

    private System.Collections.IEnumerator ShowReadButtonAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (readButton != null) readButton.SetActive(true);
    }

    private void ShowPage(int index)
    {
        var page = pages[index];
        if (pageImage != null) pageImage.sprite = page.image;
        if (pageText != null) pageText.text = page.text;
    }
}
