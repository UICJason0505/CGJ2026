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
    [SerializeField] private Transform npcChoicesContainer;
    [SerializeField] private CanvasGroup npcChoicesCanvasGroup;
    [SerializeField] private Transform playerChoicesContainer;
    [SerializeField] private CanvasGroup playerChoicesCanvasGroup;

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
    private bool _fadingIn;

    public bool IsActive { get; private set; }

    private Transform ActiveChoicesContainer =>
        _activePanel == _playerPanel ? playerChoicesContainer : npcChoicesContainer;

    private CanvasGroup ActiveChoicesCanvasGroup =>
        _activePanel == _playerPanel ? playerChoicesCanvasGroup : npcChoicesCanvasGroup;

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

        npcChoicesContainer.SetAsLastSibling();
        playerChoicesContainer.SetAsLastSibling();

        IsActive = true;
        ShowNode(_currentEvent.dialogueNodes[0].nodeId);
    }

    private void Update()
    {
        if (!IsActive) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            // 打字中 → 跳过打字
            if (_activePanel != null && _activePanel.IsTyping)
            {
                _activePanel.Complete();
                return;
            }

            // 淡入中不响应推进
            if (_fadingIn) return;

            // 打字完毕可推进 → 执行下一步
            if (_canAdvance)
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

        _canAdvance = false;
        _advanceAction = null;

        ClearAllChoices();

        var panel = PickPanel(node);
        SwitchPanel(panel, node, () => OnTypingComplete(node));

        _fadingIn = true;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeIn());
    }

    private void OnTypingComplete(DialogueNode node)
    {
        // 打字完成后才显示选项/继续/关闭
        if (node.choices != null && node.choices.Length > 0)
        {
            foreach (var choice in node.choices)
                CreateChoiceButton(choice.choiceText, () => OnChoiceSelected(choice));
            if (ActiveChoicesCanvasGroup != null)
                StartCoroutine(FadeCanvasGroup(ActiveChoicesCanvasGroup, 0f, 1f));
        }
        else if (node.nextNodeId >= 0)
        {
            _canAdvance = true;
            _advanceAction = () => OnAdvance(node.nextNodeId);
        }
        else
        {
            _canAdvance = true;
            _advanceAction = EndDialogue;
        }
    }

    private DialoguePanel PickPanel(DialogueNode node)
    {
        if (_currentEvent.dialogueType == DialogueType.InnerMonologue)
            return _playerPanel;

        if (string.IsNullOrEmpty(node.speakerName))
            return _playerPanel;

        return _npcPanel;
    }

    private void SwitchPanel(DialoguePanel newPanel, DialogueNode node, System.Action onTypingComplete)
    {
        if (_activePanel != null && _activePanel != newPanel)
            _activePanel.Hide();

        _activePanel = newPanel;

        var isPlayer = newPanel == _playerPanel;
        var portrait = isPlayer ? playerPortrait : GetPortrait(node.speakerName);
        var speaker = _currentEvent.dialogueType == DialogueType.InnerMonologue ? "" : node.speakerName;
        _activePanel.Show(speaker, node.text, portrait, !isPlayer, onTypingComplete);
    }

    private void CreateChoiceButton(string text, System.Action onClick)
    {
        var btnObj = Instantiate(choiceButtonPrefab, ActiveChoicesContainer);
        var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = text;
        var btn = btnObj.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(() => onClick());
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        StartCoroutine(ShowResponseThenAdvance(choice));
    }

    private void OnAdvance(int nextNodeId)
    {
        StartCoroutine(FadeOutThenShow(nextNodeId));
    }

    private void OnChoiceSelected(int nextNodeId)
    {
        StartCoroutine(FadeOutThenShow(nextNodeId));
    }

    private IEnumerator ShowResponseThenAdvance(DialogueChoice choice)
    {
        _canAdvance = false;
        yield return Fade(1f, 0f);

        // 选项按钮渐隐
        if (ActiveChoicesCanvasGroup != null)
            yield return FadeCanvasGroup(ActiveChoicesCanvasGroup, 1f, 0f);

        ClearAllChoices();

        if (_npcPanel != null) _npcPanel.Hide();
        _activePanel = _playerPanel;
        _playerPanel.Show("", choice.responseText, playerPortrait, true, () =>
        {
            if (_nodeMap.ContainsKey(choice.nextNodeId))
            {
                _canAdvance = true;
                _advanceAction = () => OnAdvance(choice.nextNodeId);
            }
            else
            {
                _canAdvance = true;
                _advanceAction = EndDialogue;
            }
        });

        _fadingIn = true;
        yield return Fade(0f, 1f);
        _fadingIn = false;
    }

    private IEnumerator FadeOutThenShow(int nextNodeId)
    {
        _canAdvance = false;
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
        Debug.Log("[DialogueManager] 对话结束，触发 onDialogueEnded");
        onDialogueEnded?.Invoke();
    }

    private IEnumerator FadeIn()
    {
        yield return Fade(0f, 1f);
        _fadingIn = false;
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

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, fadeCurve.Evaluate(elapsed / fadeDuration));
            yield return null;
        }
        cg.alpha = to;
    }

    private void ClearAllChoices()
    {
        if (npcChoicesCanvasGroup != null) npcChoicesCanvasGroup.alpha = 0f;
        if (playerChoicesCanvasGroup != null) playerChoicesCanvasGroup.alpha = 0f;

        foreach (Transform child in npcChoicesContainer)
            Destroy(child.gameObject);
        foreach (Transform child in playerChoicesContainer)
            Destroy(child.gameObject);
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
