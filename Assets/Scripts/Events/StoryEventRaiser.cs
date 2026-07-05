using System.Collections;
using UnityEngine;

public class StoryEventRaiser : MonoBehaviour
{
    [SerializeField] private StoryEventSO storyEvent;
    [SerializeField] private StoryEventSO[] storyEvents;
    [SerializeField] private float delayBetweenEvents = 0.1f;

    public void Raise()
    {
        StartCoroutine(RaiseSequence());
    }

    private IEnumerator RaiseSequence()
    {
        if (storyEvent != null)
        {
            TryFire(storyEvent);
            yield return new WaitForSeconds(delayBetweenEvents);
        }

        foreach (var e in storyEvents)
        {
            if (e != null)
            {
                TryFire(e);
                yield return new WaitForSeconds(delayBetweenEvents);
            }
        }
    }

    private void TryFire(StoryEventSO e)
    {
        var controller = FindObjectOfType<StorylineController>();
        if (controller != null)
            controller.TryRaiseEvent(e);
        else
            e.Raise();
    }
}
