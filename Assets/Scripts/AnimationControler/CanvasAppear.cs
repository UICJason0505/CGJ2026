using UnityEngine;

public class CanvasAppear : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StorylineController storylineController;
    [SerializeField] private GameObject canvas;

    [Header("Story Event")]
    [SerializeField] private StoryEventSO onCanvasShown;
    [SerializeField] private int goToSequence = 30;

    private CanvasGroup _cg;

    private void Awake()
    {
        if (canvas == null) return;

        _cg = canvas.GetComponent<CanvasGroup>();
        if (_cg == null) _cg = canvas.AddComponent<CanvasGroup>();

        _cg.alpha = 0f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
    }

    public void Show()
    {
        if (canvas == null) return;
        if (_cg == null) _cg = canvas.GetComponent<CanvasGroup>() ?? canvas.AddComponent<CanvasGroup>();

        _cg.alpha = 1f;
        _cg.interactable = true;
        _cg.blocksRaycasts = true;

        onCanvasShown?.Raise();
        if (storylineController != null)
            storylineController.GoToSequence(goToSequence);
    }

    public void Hide()
    {
        if (canvas == null) return;
        if (_cg == null) _cg = canvas.GetComponent<CanvasGroup>() ?? canvas.AddComponent<CanvasGroup>();

        _cg.alpha = 0f;
        _cg.interactable = false;
        _cg.blocksRaycasts = false;
    }
}
