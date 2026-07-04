using System.Collections.Generic;
using UnityEngine;

public enum TriggerMode
{
    Auto,        // 序号解锁时自动触发
    Conditional, // 由代码手动触发
    Follow       // 指定事件触发后自动跟进触发
}

[System.Serializable]
public class StoryPhaseEvent
{
    public StoryEventSO storyEvent;
    public int sequenceIndex;
    public TriggerMode triggerMode;
    public int followAfterEventIndex = -1;
    public bool repeatable;
    public bool endOfPhase;   // 触发后结束当前 Phase，进入下一个
    public bool skipInTest;
}

[System.Serializable]
public class StoryPhase
{
    public int phaseId;
    public string phaseName;
    public StoryPhaseEvent[] events;
}

public class StorylineController : MonoBehaviour
{
    [SerializeField] private StoryPhase[] phases;
    public int startPhaseId;

    private HashSet<int> _triggeredOnce;
    private int _currentSequence;
    private int _currentPhaseIndex = -1;

    public int CurrentSequenceIndex => _currentSequence;

    public StoryPhase CurrentPhase =>
        (_currentPhaseIndex >= 0 && _currentPhaseIndex < phases.Length)
            ? phases[_currentPhaseIndex]
            : null;

    private void Awake()
    {
        _triggeredOnce = new HashSet<int>();
        _currentPhaseIndex = -1;
        _currentSequence = -1;
    }

    public void NextPhase()
    {
        GoToPhaseById(startPhaseId);
        startPhaseId++;
    }

    public void GoToPhase(int index)
    {
        _currentPhaseIndex = index;
        _currentSequence = -1;
        _triggeredOnce.Clear();
        startPhaseId = CurrentPhase?.phaseId ?? 0;
        Debug.Log($"[Storyline] 跳转到阶段索引 [{_currentPhaseIndex}] Id={startPhaseId} \"{CurrentPhase?.phaseName}\"");
        AdvanceSequence();
    }

    /// <summary>通过 phaseId 跳转到指定阶段</summary>
    public void GoToPhaseById(int id)
    {
        for (int i = 0; i < phases.Length; i++)
        {
            if (phases[i].phaseId == id)
            {
                _currentPhaseIndex = i;
                _currentSequence = -1;
                _triggeredOnce.Clear();
                Debug.Log($"[Storyline] 进入阶段 Id={id} \"{CurrentPhase?.phaseName}\"");
                AdvanceSequence();
                return;
            }
        }
        Debug.LogWarning($"[Storyline] 未找到 phaseId={id} 的阶段");
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

        if (entry.skipInTest)
        {
            Debug.Log($"[Storyline] 事件 {eventIndex} 跳过（skipInTest）");
            AdvanceSequence();
            return;
        }

        // 可重复触发：不限序号，无限触发，首次触发推进序号
        if (entry.repeatable)
        {
            entry.storyEvent.Raise();
            TriggerFollowEvents(eventIndex);
            if (!_triggeredOnce.Contains(eventIndex))
            {
                _triggeredOnce.Add(eventIndex);
                if (entry.endOfPhase)
                {
                    Debug.Log($"[Storyline] Phase[{_currentPhaseIndex}] Id={startPhaseId} 结束");
                    NextPhase();
                }
                else
                {
                    Debug.Log($"[Storyline] 重复触发事件 [{eventIndex}]（首次，推进序号）");
                    AdvanceSequence();
                }
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
        TriggerFollowEvents(eventIndex);

        if (entry.endOfPhase)
        {
            Debug.Log($"[Storyline] Phase[{_currentPhaseIndex}] Id={startPhaseId} 结束");
            NextPhase();
        }
        else
        {
            AdvanceSequence();
        }
    }

    private void TriggerFollowEvents(int sourceIndex)
    {
        var phase = CurrentPhase;
        if (phase == null) return;

        for (int i = 0; i < phase.events.Length; i++)
        {
            var e = phase.events[i];
            if (e.triggerMode != TriggerMode.Follow) continue;
            if (e.followAfterEventIndex != sourceIndex) continue;
            if (e.storyEvent == null) continue;
            if (e.skipInTest) continue;

            Debug.Log($"[Storyline] Follow 触发事件 [{i}]（跟随 [{sourceIndex}]）");
            e.storyEvent.Raise();
        }
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
        if (storyEvent == null)
        {
            Debug.LogWarning("[Storyline] TryRaiseEvent: storyEvent 为空");
            return;
        }
        var phase = CurrentPhase;
        if (phase == null)
        {
            Debug.LogWarning($"[Storyline] TryRaiseEvent: 当前无阶段，无法触发 {storyEvent.name}");
            return;
        }

        for (int i = 0; i < phase.events.Length; i++)
        {
            if (phase.events[i].storyEvent == storyEvent)
            {
                RaiseEvent(i);
                return;
            }
        }

        Debug.LogWarning($"[Storyline] TryRaiseEvent: 当前阶段未找到事件 {storyEvent.name}，请将其加入 Phase Id={startPhaseId} 的 Events 列表");
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
            if (entry.skipInTest) continue;

            _triggeredOnce.Add(i);
            entry.storyEvent.Raise();
            Debug.Log($"[Storyline] 自动触发事件 [{i}]");
        }
    }
}
