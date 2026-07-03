using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StoryEventSO))]
public class StoryEventSOEditor : Editor
{
    private SerializedProperty _outcomeType;
    private SerializedProperty _description;
    private SerializedProperty _dialogueType;
    private SerializedProperty _dialogueNodes;
    private SerializedProperty _popupSprite;
    private SerializedProperty _popupDescription;
    private SerializedProperty _animationTriggerName;

    private void OnEnable()
    {
        _outcomeType = serializedObject.FindProperty("outcomeType");
        _description = serializedObject.FindProperty("description");
        _dialogueType = serializedObject.FindProperty("dialogueType");
        _dialogueNodes = serializedObject.FindProperty("dialogueNodes");
        _popupSprite = serializedObject.FindProperty("popupSprite");
        _popupDescription = serializedObject.FindProperty("popupDescription");
        _animationTriggerName = serializedObject.FindProperty("animationTriggerName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_outcomeType);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_description);

        var type = (EventOutcomeType)_outcomeType.enumValueIndex;

        switch (type)
        {
            case EventOutcomeType.Dialogue:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dialogue Data", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_dialogueType);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_dialogueNodes, new GUIContent("Dialogue Nodes"), true);
                break;

            case EventOutcomeType.Description:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Popup Description", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_popupSprite, new GUIContent("Sprite"));
                EditorGUILayout.LabelField("Description Text");
                _popupDescription.stringValue = EditorGUILayout.TextArea(
                    _popupDescription.stringValue,
                    GUILayout.Height(80));
                break;

            case EventOutcomeType.Animation:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Animation Trigger", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_animationTriggerName, new GUIContent("Trigger Name"));
                break;

            case EventOutcomeType.Function:
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No extra data — wire the Function UnityEvent on StoryEventListener.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
