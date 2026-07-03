using UnityEngine;

public class StorylineController : MonoBehaviour
{
    [SerializeField] private StoryEventSO[] storyEvents;

    public void RaiseEvent(StoryEventSO storyEvent)
    {
        if (storyEvent != null)
            storyEvent.Raise();
    }

    public void RaiseEventByIndex(int index)
    {
        if (index >= 0 && index < storyEvents.Length)
            storyEvents[index].Raise();
    }
}
