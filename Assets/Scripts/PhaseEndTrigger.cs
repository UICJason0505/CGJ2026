using UnityEngine;

public class PhaseEndTrigger : MonoBehaviour
{
    [SerializeField] private StoryEventSO endEvent;
    [SerializeField] private StorylineController storylineController;

    public void EndPhase()
    {
        if (storylineController != null)
        {
            storylineController.startPhaseId++;
            storylineController.GoToPhaseById(storylineController.startPhaseId);
        }
    }
}
