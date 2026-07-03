using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class SpeakerPortrait
{
    public string speakerName;
    public Sprite portrait;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Prefabs")]
    [SerializeField] private Transform dialogueCanvas;
    [SerializeField] private DialoguePanel npcPanelPrefab;
    [SerializeField] private DialoguePanel playerPanelPrefab;

    [Header("Choice")]
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choicesContainer;

    [Header("Settings")]
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private SpeakerPortrait[] speakerPortraits;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public UnityEvent onDialogueEnded;

    private StoryEventSO _currentEvent;
    private Dictionary<int, DialogueNode> _nodeMap;
    private DialoguePanel _npcPanel;
    private DialoguePanel _playerPanel;
    private DialoguePanel _activePanel;
    private Coroutine _fadeRoutine;
    private System.Action _advanceAction;
    private bool _canAdvance;

    public bool IsActive { get; private set; }

    public void StartDialogue(StoryEventSO storyEvent)
    {
        if (storyEvent == null || storyEvent.dialogueNodes == null || storyEvent.dialogueNodes.Length == 0)
        {
            EndDialogue();
            return;
        }

        _currentEvent = storyEvent;
        _nodeMap = new Dictionary<int, DialogueNode>();
        foreach (var node in storyEvent.dialogueNodes)
            _nodeMap[node.nodeId] = node;

        // 实例化两个面板，默认隐藏
        if (_npcPanel == null && npcPanelPrefab != null)
        {
            _npcPanel = Instantiate(npcPanelPrefab, dialogueCanvas);
            _npcPanel.Hide();
        }
        if (_playerPanel == null && playerPanelPrefab != null)
        {
            _playerPanel = Instantiate(playerPanelPrefab, dialogueCanvas);
            _playerPanel.Hide();
        }

        // 选项容器置顶，防止被面板遮挡
        choicesContainer.SetAsLastSibling();

        IsActive = true;
        ShowNode(_currentEvent.dialogueNodes[0].nodeId);
    }

    private void Update()
    {
        if (!IsActive || !_canAdvance) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            _advanceAction?.Invoke();
        }
    }

    private void ShowNode(int nodeId)
    {
        if (!_nodeMap.TryGetValue(nodeId, out var node))
        {
            EndDialogue();
            return;
        }

        // 选择面板
        var panel = PickPanel(node);
        SwitchPanel(panel, node);

        // 刷新选项
        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);

        if (node.choices != null && node.choices.Length > 0)
        {
            _canAdvance = false;
            foreach (var choice in node.choices)
                CreateChoiceButton(choice.choiceText, () => OnChoiceSelected(choice));
        }
        else if (node.nextNodeId >= 0)
        {
            _canAdvance = true;
            _advanceAction = () => OnChoiceSelected(node.nextNodeId);
        }
        else
        {
            _canAdvance = true;
            _advanceAction = EndDialogue;
        }

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeIn());
    }

    private DialoguePanel PickPanel(DialogueNode node)
    {
        // 内心独白 → 始终用主角面板
        if (_currentEvent.dialogueType == DialogueType.InnerMonologue)
            return _playerPanel;

        // 对话 → 有 name 的是 NPC，没有的是主角
        if (string.IsNullOrEmpty(node.speakerName))
            return _playerPanel;

        return _npcPanel;
    }

    private void SwitchPanel(DialoguePanel newPanel, DialogueNode node)
    {
        if (_activePanel != null && _activePanel != newPanel)
            _activePanel.Hide();

        _activePanel = newPanel;

        var isPlayer = newPanel == _playerPanel;
        var portrait = isPlayer ? playerPortrait : GetPortrait(node.speakerName);
        var speaker = _currentEvent.dialogueType == DialogueType.InnerMonologue ? "" : node.speakerName;
        _activePanel.Show(speaker, node.text, portrait);
    }

    private void CreateChoiceButton(string text, System.Action onClick)
    {
        var btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
        var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = text;
        var btn = btnObj.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(() => onClick());
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        StartCoroutine(ShowResponseThenAdvance(choice));
    }

    private void OnChoiceSelected(int nextNodeId)
    {
        StartCoroutine(FadeOutThenShow(nextNodeId));
    }

    private IEnumerator ShowResponseThenAdvance(DialogueChoice choice)
    {
        yield return Fade(1f, 0f);

        // 切到主角面板显示回应文本
        if (_npcPanel != null) _npcPanel.Hide();
        _activePanel = _playerPanel;
        _playerPanel.Show("", choice.responseText, playerPortrait);

        // 清除旧选项
        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);

        _canAdvance = true;
        if (_nodeMap.ContainsKey(choice.nextNodeId))
            _advanceAction = () => OnChoiceSelected(choice.nextNodeId);
        else
            _advanceAction = EndDialogue;

        yield return Fade(0f, 1f);
    }

    private IEnumerator FadeOutThenShow(int nextNodeId)
    {
        yield return Fade(1f, 0f);
        ShowNode(nextNodeId);
    }

    public void EndDialogue()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutAndClose());
    }

    private IEnumerator FadeOutAndClose()
    {
        yield return Fade(1f, 0f);
        if (_npcPanel != null) Destroy(_npcPanel.gameObject);
        if (_playerPanel != null) Destroy(_playerPanel.gameObject);
        _npcPanel = null;
        _playerPanel = null;
        _activePanel = null;
        IsActive = false;
        _currentEvent = null;
        _nodeMap = null;
        onDialogueEnded?.Invoke();
    }

    private IEnumerator FadeIn()
    {
        yield return Fade(0f, 1f);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeDuration);
            var alpha = Mathf.Lerp(from, to, t);
            if (_npcPanel != null) _npcPanel.SetAlpha(alpha);
            if (_playerPanel != null) _playerPanel.SetAlpha(alpha);
            yield return null;
        }
        if (_npcPanel != null) _npcPanel.SetAlpha(to);
        if (_playerPanel != null) _playerPanel.SetAlpha(to);
    }

    private Sprite GetPortrait(string speakerName)
    {
        foreach (var sp in speakerPortraits)
        {
            if (sp.speakerName == speakerName)
                return sp.portrait;
        }
        return null;
    }
}
