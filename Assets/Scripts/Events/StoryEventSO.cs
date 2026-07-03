using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventOutcomeType
{
    Dialogue,     // 多段对话 / 内心独白
    Description,  // 弹出描述
    Animation,    // 动画播放
    Function      // 触发函数
}

public enum DialogueType
{
    Conversation,    // 对话
    InnerMonologue   // 内心独白
}

[Serializable]
public class DialogueChoice
{
    public string choiceText;    // 选项按钮上的文字
    [TextArea(2, 3)]
    public string responseText;  // 选择后主角说的详细文本
    public int nextNodeId;       // 跳转到哪个节点
}

[Serializable]
public class DialogueNode
{
    public int nodeId;
    [TextArea(2, 5)]
    public string text;
    public string speakerName;       // 对话时填，独白时留空
    public DialogueChoice[] choices; // 选项回应，可空
    public int nextNodeId = -1;      // 下一段对话的 nodeId，-1 表示终点
}

[CreateAssetMenu(menuName = "Storyline/Story Event", fileName = "NewStoryEvent")]
public class StoryEventSO : ScriptableObject
{
    [Header("Event Outcome")]
    public EventOutcomeType outcomeType;

    [Header("Notes")]
    [TextArea(2, 5)]
    public string description;

    [Header("Dialogue Data")]
    public DialogueType dialogueType;
    public DialogueNode[] dialogueNodes;

    [Header("Popup Description")]
    public Sprite popupSprite;
    [TextArea(3, 10)]
    public string popupDescription;

    [Header("Animation")]
    public string animationTriggerName;

    private readonly List<StoryEventListener> _listeners = new();

    public void Raise()
    {
        Debug.Log($"[EventSO] {name}: Raise 被调用，监听者数量={_listeners.Count}");
        for (int i = _listeners.Count - 1; i >= 0; i--)
            _listeners[i].OnEventRaised(this);
    }

    public void Register(StoryEventListener listener) => _listeners.Add(listener);
    public void Unregister(StoryEventListener listener) => _listeners.Remove(listener);
}
