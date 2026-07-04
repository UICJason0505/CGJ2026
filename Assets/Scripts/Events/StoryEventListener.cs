using UnityEngine;
using UnityEngine.Events;

public class StoryEventListener : MonoBehaviour
{
    [SerializeField] private StoryEventSO[] storyEvents;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private StorylineController storylineController;

    [Header("Description")]
    [SerializeField] private Transform descriptionCanvas;
    [SerializeField] private GameObject descriptionTemplate;

    [Header("Outcome Handlers")]
    [SerializeField] private UnityEvent<StoryEventSO> onDescription;
    [SerializeField] private UnityEvent<string> onAnimation;
    [SerializeField] private UnityEvent onFunction;

    private void Start()
    {
        if (dialogueManager != null && storylineController != null)
            dialogueManager.onDialogueEnded.AddListener(() =>
            {
                Debug.Log("[Listener] 对话结束，推进序号");
                storylineController.ForceNextSequence();
            });
    }

    private void OnEnable()
    {
        foreach (var e in storyEvents)
        {
            if (e != null) e.Register(this);
        }
    }

    private void OnDisable()
    {
        foreach (var e in storyEvents)
        {
            if (e != null) e.Unregister(this);
        }
    }

    public void ClearDescriptions()
    {
        if (descriptionCanvas == null) return;
        for (int i = descriptionCanvas.childCount - 1; i >= 0; i--)
            Destroy(descriptionCanvas.GetChild(i).gameObject);
    }

    public bool IsBusy =>
        (descriptionCanvas != null && descriptionCanvas.childCount > 0)
        || (dialogueManager != null && dialogueManager.IsActive);

    public void OnEventRaised(StoryEventSO raisedEvent)
    {
        if (IsBusy)
        {
            Debug.Log($"[Listener] 忙碌中，拦截事件: {raisedEvent.name}");
            return;
        }

        Debug.Log($"[Listener] 收到事件: {raisedEvent.name}, outcomeType={raisedEvent.outcomeType}");
        switch (raisedEvent.outcomeType)
        {
            case EventOutcomeType.Dialogue:
                if (dialogueManager != null)
                    dialogueManager.StartDialogue(raisedEvent);
                break;

            case EventOutcomeType.Description:
                if (descriptionTemplate != null)
                {
                    var instance = Instantiate(descriptionTemplate, descriptionCanvas);
                    var popup = instance.GetComponent<DescriptionPopup>();
                    if (popup != null)
                        popup.Init(raisedEvent.popupSprite, raisedEvent.popupDescription, () =>
                        {
                            Debug.Log("[Listener] 描述弹窗关闭，推进序号");
                            storylineController?.ForceNextSequence();
                        });
                }
                onDescription?.Invoke(raisedEvent);
                break;

            case EventOutcomeType.Animation:
                onAnimation?.Invoke(raisedEvent.animationTriggerName);
                break;

            case EventOutcomeType.Function:
                onFunction?.Invoke();
                break;
        }
    }
}
