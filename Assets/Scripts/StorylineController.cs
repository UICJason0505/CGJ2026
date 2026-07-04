using System.Collections.Generic;
using UnityEngine;

public enum TriggerMode
{
    Auto,        // 序号解锁时自动触发
    Conditional  // 由代码手动触发
}

[System.Serializable]
public class StoryPhaseEvent
{
    public StoryEventSO storyEvent;
    public int sequenceIndex;
    public bool repeatable;
    public TriggerMode triggerMode;
}

[System.Serializable]
public class StoryPhase
{
    public string phaseName;
    public StoryPhaseEvent[] events;
}

public class StorylineController : MonoBehaviour
{
    [SerializeField] private StoryPhase[] phases;

    private HashSet<int> _triggeredOnce;
    private int _currentSequence;

    public int CurrentPhaseIndex { get; private set; }
    public int CurrentSequenceIndex => _currentSequence;

    public StoryPhase CurrentPhase =>
        (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < phases.Length)
            ? phases[CurrentPhaseIndex]
            : null;

    private void Awake()
    {
        _triggeredOnce = new HashSet<int>();
        CurrentPhaseIndex = -1;
        _currentSequence = -1;
    }

    public void NextPhase()
    {
        CurrentPhaseIndex++;
        _currentSequence = -1;
        _triggeredOnce.Clear();
        Debug.Log($"[Storyline] 进入阶段 [{CurrentPhaseIndex}] \"{CurrentPhase?.phaseName}\"");
        AdvanceSequence();
    }

    public void GoToPhase(int index)
    {
        CurrentPhaseIndex = index;
        _currentSequence = -1;
        _triggeredOnce.Clear();
        Debug.Log($"[Storyline] 跳转到阶段 [{CurrentPhaseIndex}] \"{CurrentPhase?.phaseName}\"");
        AdvanceSequence();
    }

    public void RaiseEvent(int eventIndex)
    {
        var phase = CurrentPhase;
        if (phase == null)
        {
            Debug.LogWarning("[Storyline] 当前无阶段");
            return;
        }

        if (eventIndex < 0 || eventIndex >= phase.events.Length)
        {
            Debug.LogWarning($"[Storyline] 无事件索引 {eventIndex}");
            return;
        }

        var entry = phase.events[eventIndex];
        if (entry.storyEvent == null) return;

        // 可重复触发：不限序号，无限触发，首次触发推进序号
        if (entry.repeatable)
        {
            entry.storyEvent.Raise();
            if (!_triggeredOnce.Contains(eventIndex))
            {
                _triggeredOnce.Add(eventIndex);
                Debug.Log($"[Storyline] 重复触发事件 [{eventIndex}]（首次，推进序号）");
                AdvanceSequence();
            }
            else
            {
                Debug.Log($"[Storyline] 重复触发事件 [{eventIndex}]（非首次，序号不变）");
            }
            return;
        }

        if (entry.sequenceIndex > _currentSequence)
        {
            Debug.LogWarning($"[Storyline] 事件 {eventIndex} 序号 {entry.sequenceIndex} 尚未解锁（当前 {_currentSequence}）");
            return;
        }

        if (_triggeredOnce.Contains(eventIndex))
        {
            Debug.LogWarning($"[Storyline] 事件 {eventIndex} 已触发过");
            return;
        }

        _triggeredOnce.Add(eventIndex);
        entry.storyEvent.Raise();
        Debug.Log($"[Storyline] 触发事件 [{eventIndex}]");
        AdvanceSequence();
    }

    /// <summary>检查事件在当前阶段是否可触发（序号已解锁且未被触发过或可重复）</summary>
    public bool CanTriggerEvent(StoryEventSO storyEvent)
    {
        if (storyEvent == null) return false;
        var phase = CurrentPhase;
        if (phase == null) return false;

        for (int i = 0; i < phase.events.Length; i++)
        {
            var entry = phase.events[i];
            if (entry.storyEvent != storyEvent) continue;
            if (entry.repeatable) return true;
            if (entry.sequenceIndex > _currentSequence) return false; // 未解锁
            return !_triggeredOnce.Contains(i);
        }
        return false; // 未在当前阶段找到
    }

    /// <summary>通过 StoryEventSO 引用触发（供 Dream/Button 等使用），自动查索引并校验序号</summary>
    public void TryRaiseEvent(StoryEventSO storyEvent)
    {
        if (storyEvent == null) return;
        var phase = CurrentPhase;
        if (phase == null) return;

        for (int i = 0; i < phase.events.Length; i++)
        {
            if (phase.events[i].storyEvent == storyEvent)
            {
                RaiseEvent(i);
                return;
            }
        }

        Debug.LogWarning($"[Storyline] 当前阶段未找到事件: {storyEvent.name}");
    }

    /// <summary>强制跳到下一个序号</summary>
    public void ForceNextSequence()
    {
        AdvanceSequence();
    }

    private void AdvanceSequence()
    {
        _currentSequence++;
        Debug.Log($"[Storyline] 序号推进 → {_currentSequence}");
        TriggerAutoEvents();
    }

    private void TriggerAutoEvents()
    {
        var phase = CurrentPhase;
        if (phase == null) return;

        for (int i = 0; i < phase.events.Length; i++)
        {
            var entry = phase.events[i];
            if (entry.storyEvent == null) continue;
            if (entry.triggerMode != TriggerMode.Auto) continue;
            if (entry.sequenceIndex != _currentSequence) continue;
            if (!entry.repeatable && _triggeredOnce.Contains(i)) continue;

            _triggeredOnce.Add(i);
            entry.storyEvent.Raise();
            Debug.Log($"[Storyline] 自动触发事件 [{i}]");
        }
    }
}
