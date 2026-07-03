using UnityEngine;
using UnityEngine.Events;

public class StoryEventListener : MonoBehaviour
{
    [SerializeField] private StoryEventSO[] storyEvents;

    [Header("Description")]
    [SerializeField] private Transform descriptionCanvas;
    [SerializeField] private GameObject descriptionTemplate;

    [Header("Outcome Handlers")]
    [SerializeField] private UnityEvent<StoryEventSO> onDialogue;
    [SerializeField] private UnityEvent<StoryEventSO> onDescription;
    [SerializeField] private UnityEvent<string> onAnimation;
    [SerializeField] private UnityEvent onFunction;

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

    public bool IsDescriptionActive =>
        descriptionCanvas != null && descriptionCanvas.childCount > 0;

    public void OnEventRaised(StoryEventSO raisedEvent)
    {
        if (IsDescriptionActive)
        {
            Debug.Log($"[Listener] Description 弹窗未关闭，拦截事件: {raisedEvent.name}");
            return;
        }

        Debug.Log($"[Listener] 收到事件: {raisedEvent.name}, outcomeType={raisedEvent.outcomeType}");
        switch (raisedEvent.outcomeType)
        {
            case EventOutcomeType.Dialogue:
                onDialogue?.Invoke(raisedEvent);
                break;
            case EventOutcomeType.Description:
                Debug.Log($"[Listener] Description 分支: template={descriptionTemplate != null}, canvas={descriptionCanvas != null}");
                if (descriptionTemplate != null)
                {
                    var instance = Instantiate(descriptionTemplate, descriptionCanvas);
                    Debug.Log($"[Listener] 实例化了 Prefab: {instance.name}");
                    var popup = instance.GetComponent<DescriptionPopup>();
                    if (popup != null)
                        popup.Init(raisedEvent.popupSprite, raisedEvent.popupDescription);
                    else
                        Debug.LogWarning($"[Listener] Prefab 上没找到 DescriptionPopup 组件！", instance);
                }
                else
                {
                    Debug.LogWarning("[Listener] descriptionTemplate 为空，请拖入 Prefab！", this);
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
