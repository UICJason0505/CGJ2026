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
        Debug.Log($"[Raiser] {name}: 触发事件 {storyEvent.name}");
        storyEvent.Raise();
    }
}
