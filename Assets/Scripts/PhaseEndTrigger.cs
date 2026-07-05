using UnityEngine;

public class PhaseEndTrigger : MonoBehaviour
{
    [SerializeField] private StoryEventSO endEvent;
    [SerializeField] private StorylineController storylineController;

    public void EndPhase()
    {
        if (storylineController == null) return;
        var nextIndex = storylineController._currentPhaseIndex + 1;
        if (nextIndex >= storylineController.Phases.Length)
        {
            Debug.LogWarning("[PhaseEndTrigger] 已经是最后一个 Phase");
            return;
        }
        var nextId = storylineController.Phases[nextIndex].phaseId;
        Debug.Log($"[PhaseEndTrigger] 阶段 {storylineController.startPhaseId} → {nextId}");
        storylineController.startPhaseId = nextId;
        storylineController.GoToPhaseById(nextId);
    }
}
