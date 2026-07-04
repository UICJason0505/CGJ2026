using UnityEngine;

public class StoryEventRaiser : MonoBehaviour
{
    [SerializeField] private StoryEventSO storyEvent;

    public void Raise()
    {
        if (storyEvent == null)
        {
            Debug.LogWarning($"[Raiser] {name}: storyEvent 为空，未赋值！", this);
            return;
        }

        var controller = FindObjectOfType<StorylineController>();
        if (controller != null)
        {
            controller.TryRaiseEvent(storyEvent);
        }
        else
        {
            Debug.LogWarning($"[Raiser] {name}: 场景中无 StorylineController，直接触发", this);
            storyEvent.Raise();
        }
    }
}
