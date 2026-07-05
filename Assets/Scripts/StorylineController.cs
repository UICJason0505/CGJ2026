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
    public int followAfterSequence = -1; // 指定序号完成后自动 Follow
    public bool repeatable;
    public bool endOfPhase;   // 触发后结束当前 Phase，进入下一个
    public bool skipInTest;
}

[System.Serializable]
public class StoryPhase
{
    public int phaseId;
    public string phaseName;
    public Canvas canvas;
    public StoryPhaseEvent[] events;
}

public class StorylineController : MonoBehaviour
{
    [SerializeField] private StoryPhase[] phases;
    public int startPhaseId;

    [Header("Phase Transition")]
    [SerializeField] private float phaseFadeDuration = 0.5f;
    [SerializeField] private AnimationCurve phaseFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private HashSet<int> _triggeredOnce;
    private int _currentSequence;
    public int _currentPhaseIndex = -1;
    public StoryPhase[] Phases => phases;
    private Coroutine _phaseFadeRoutine;

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

        foreach (var p in phases)
        {
            if (p.canvas != null)
            {
                p.canvas.gameObject.SetActive(false);
                var cg = p.canvas.GetComponent<CanvasGroup>();
                if (cg == null) cg = p.canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    public void NextPhase()
    {
        if (_currentPhaseIndex < 0)
        {
            GoToPhaseById(startPhaseId);
            return;
        }

        var nextIndex = _currentPhaseIndex + 1;
        if (nextIndex < phases.Length)
            GoToPhaseById(phases[nextIndex].phaseId);
        else
            Debug.LogWarning("[Storyline] 已经是最后一个 Phase，无法推进");
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
                if (_phaseFadeRoutine != null) StopCoroutine(_phaseFadeRoutine);
                _phaseFadeRoutine = StartCoroutine(TransitionPhase(i));
                return;
            }
        }
        Debug.LogWarning($"[Storyline] 未找到 phaseId={id} 的阶段");
    }

    private System.Collections.IEnumerator TransitionPhase(int targetIndex)
    {
        var oldPhase = CurrentPhase;

        // 渐隐当前 Canvas
        if (oldPhase?.canvas != null)
        {
            var cg = oldPhase.canvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = oldPhase.canvas.gameObject.AddComponent<CanvasGroup>();
            Debug.Log($"[Storyline] 渐隐 Canvas: {oldPhase.canvas.name}");
            yield return FadeCanvasGroup(cg, cg.alpha, 0f);
            oldPhase.canvas.gameObject.SetActive(false);
        }

        // 清掉旧阶段的弹窗
        FindObjectOfType<StoryEventListener>()?.ClearDescriptions();

        // 切换
        _currentPhaseIndex = targetIndex;
        _currentSequence = -1;
        _triggeredOnce.Clear();
        startPhaseId = phases[targetIndex].phaseId;

        var newPhase = phases[targetIndex];

        // 渐显新 Canvas
        if (newPhase.canvas != null)
        {
            newPhase.canvas.gameObject.SetActive(true);
            var cg = newPhase.canvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = newPhase.canvas.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            Debug.Log($"[Storyline] 渐显 Canvas: {newPhase.canvas.name}");
            yield return FadeCanvasGroup(cg, 0f, 1f);
        }
        else
        {
            Debug.LogWarning($"[Storyline] Phase Id={startPhaseId} 未设置 Canvas");
        }

        Debug.Log($"[Storyline] 进入阶段 Id={startPhaseId} \"{newPhase.phaseName}\"");
        AdvanceSequence();
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < phaseFadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, phaseFadeCurve.Evaluate(elapsed / phaseFadeDuration));
            yield return null;
        }
        cg.alpha = to;
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

        if (entry.triggerMode != TriggerMode.Follow && entry.sequenceIndex > _currentSequence)
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
        Debug.Log($"[Storyline] 触发事件 [{eventIndex}]，类型={entry.storyEvent.outcomeType}");

        // Follow 事件不推进序号
        if (entry.triggerMode == TriggerMode.Follow)
            return;

        // 对话和描述事件不立即推进，等完成回调（onDialogueEnded / DescriptionPopup.Close）
        var isAsync = entry.storyEvent.outcomeType == EventOutcomeType.Dialogue
                   || entry.storyEvent.outcomeType == EventOutcomeType.Description;

        if (!isAsync)
        {
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
    }

    private void TriggerFollowEvents(int completedSequence)
    {
        var phase = CurrentPhase;
        if (phase == null) return;

        for (int i = 0; i < phase.events.Length; i++)
        {
            var e = phase.events[i];
            if (e.triggerMode != TriggerMode.Follow) continue;
            if (e.followAfterSequence != completedSequence) continue;
            if (e.storyEvent == null) continue;
            if (e.skipInTest) continue;

            Debug.Log($"[Storyline] Follow 触发事件 [{i}]（序号 {completedSequence} 完成后）");
            RaiseEvent(i);
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
        var completedSequence = _currentSequence;
        _currentSequence++;
        Debug.Log($"[Storyline] 序号推进：{completedSequence} → {_currentSequence}");
        TriggerFollowEvents(completedSequence);
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

            if (entry.endOfPhase)
            {
                Debug.Log($"[Storyline] Phase[{_currentPhaseIndex}] Id={startPhaseId} 结束（Auto）");
                NextPhase();
                return;
            }
        }
    }
}
