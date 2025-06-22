using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuestSystem.Inspectors
{
    using QuestSystem;
    using QuestSystem.ScriptableObjects;
    using Sirenix.OdinInspector;

    //[CustomEditor(typeof(Quest))]
    public class QuestSystemInspector : Editor
    {
        ///* Quest Scriptable Objects */
        //private SerializedProperty questContainerProperty;
        //private SerializedProperty questGroupProperty;
        //private SerializedProperty objectiveProperty;

        ///* Filters */
        //private SerializedProperty groupedQuestsProperty;
        //private SerializedProperty startingQuestsOnlyProperty;

        ///* Indexes */
        //private SerializedProperty selectedQuestGroupIndexProperty;
        //private SerializedProperty selectedQuestIndexProperty;

        //private void OnEnable()
        //{
        //    questContainerProperty = serializedObject.FindProperty("questContainer");
        //    questGroupProperty = serializedObject.FindProperty("questGroup");
        //    objectiveProperty = serializedObject.FindProperty("objective");

        //    groupedQuestsProperty = serializedObject.FindProperty("groupedQuests");
        //    startingQuestsOnlyProperty = serializedObject.FindProperty("startingQuestsOnly");

        //    selectedQuestGroupIndexProperty = serializedObject.FindProperty("selectedQuestGroupIndex");
        //    selectedQuestIndexProperty = serializedObject.FindProperty("selectedQuestIndex");
        //}

        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();

        //    DrawQuestContainerArea();

        //    if (questContainerProperty.objectReferenceValue == null)
        //    {
        //        serializedObject.ApplyModifiedProperties();

        //        Debug.LogWarning("Quest Container is not assigned. Please assign a valid Quest Container in the Inspector.");

        //        return; // Stop further drawing
        //    }

        //    QuestSystemQuestContainerSO currentQuestContainer = (QuestSystemQuestContainerSO)questContainerProperty.objectReferenceValue;

        //    if (currentQuestContainer == null)
        //    {
        //        StopDrawing("Select a Quest Container to see the rest of the Inspector.");
        //        return;
        //    }

        //    DrawFiltersArea();

        //    bool currentGroupedQuestsFilter = groupedQuestsProperty.boolValue;
        //    bool currentStartingQuestsOnlyFilter = startingQuestsOnlyProperty.boolValue;

        //    List<string> questNames;
        //    string questFolderPath = $"Assets/QuestSystem/Quests/{currentQuestContainer.name}";
        //    string questInfoMessage;

        //    if (currentGroupedQuestsFilter)
        //    {
        //        List<string> questGroupNames = currentQuestContainer.GetQuestGroupNames();

        //        if (questGroupNames.Count == 0)
        //        {
        //            StopDrawing("There are no Quest Groups in this Quest Container.");
        //            return;
        //        }

        //        DrawQuestGroupArea(currentQuestContainer, questGroupNames);

        //        QuestSystemQuestGroupSO questGroup = (QuestSystemQuestGroupSO)questGroupProperty.objectReferenceValue;

        //        questNames = currentQuestContainer.GetGroupedQuestNames(questGroup, currentStartingQuestsOnlyFilter);
        //        questFolderPath += $"/Groups/{questGroup.name}/Quests";
        //        questInfoMessage = "There are no" + (currentStartingQuestsOnlyFilter ? " Starting" : "") + " Quests in this Quest Group.";
        //    }
        //    else
        //    {
        //        questNames = currentQuestContainer.GetUngroupedQuestNames(currentStartingQuestsOnlyFilter);
        //        questFolderPath += "/Global/Quests";
        //        questInfoMessage = "There are no" + (currentStartingQuestsOnlyFilter ? " Starting" : "") + " Ungrouped Quests in this Quest Container.";
        //    }

        //    if (questNames.Count == 0)
        //    {
        //        StopDrawing(questInfoMessage);
        //        return;
        //    }

        //    DrawQuestArea(questNames, questFolderPath);

        //    serializedObject.ApplyModifiedProperties();
        //}

        //private void DrawQuestContainerArea()
        //{
        //    EditorGUILayout.LabelField("Quest Container", EditorStyles.boldLabel);

        //    EditorGUILayout.PropertyField(questContainerProperty);

        //    if (questContainerProperty.objectReferenceValue == null)
        //    {
        //        EditorGUILayout.HelpBox("Please assign a Quest Container.", MessageType.Warning);
        //        EditorGUILayout.Space();
        //        return;
        //    }

        //    EditorGUILayout.Space();
        //}


        //private void DrawFiltersArea()
        //{
        //    EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        //    EditorGUILayout.PropertyField(groupedQuestsProperty, new GUIContent("Grouped Quests"));
        //    EditorGUILayout.PropertyField(startingQuestsOnlyProperty, new GUIContent("Starting Quests Only"));
        //    EditorGUILayout.Space();
        //}

        //private void DrawQuestGroupArea(QuestSystemQuestContainerSO questContainer, List<string> questGroupNames)
        //{
        //    EditorGUILayout.LabelField("Quest Group", EditorStyles.boldLabel);

        //    int oldIndex = selectedQuestGroupIndexProperty.intValue;
        //    QuestSystemQuestGroupSO oldGroup = (QuestSystemQuestGroupSO)questGroupProperty.objectReferenceValue;
        //    string oldGroupName = oldGroup == null ? "" : oldGroup.name;

        //    UpdateIndexOnNamesListUpdate(questGroupNames, selectedQuestGroupIndexProperty, oldIndex, oldGroupName, oldGroup == null);

        //    selectedQuestGroupIndexProperty.intValue = EditorGUILayout.Popup("Quest Group", selectedQuestGroupIndexProperty.intValue, questGroupNames.ToArray());

        //    string selectedGroupName = questGroupNames[selectedQuestGroupIndexProperty.intValue];
        //    QuestSystemQuestGroupSO selectedGroup = AssetDatabase.LoadAssetAtPath<QuestSystemQuestGroupSO>($"Assets/QuestSystem/Quests/{questContainer.name}/Groups/{selectedGroupName}.asset");
        //    questGroupProperty.objectReferenceValue = selectedGroup;

        //    EditorGUILayout.Space();
        //}

        //private void DrawQuestArea(List<string> questNames, string questFolderPath)
        //{
        //    EditorGUILayout.LabelField("Quest", EditorStyles.boldLabel);

        //    int oldIndex = selectedQuestIndexProperty.intValue;
        //    QuestSystemQuestSO oldQuest = (QuestSystemQuestSO)objectiveProperty.objectReferenceValue;
        //    string oldQuestName = oldQuest == null ? "" : oldQuest.name;

        //    UpdateIndexOnNamesListUpdate(questNames, selectedQuestIndexProperty, oldIndex, oldQuestName, oldQuest == null);

        //    selectedQuestIndexProperty.intValue = EditorGUILayout.Popup("Quest", selectedQuestIndexProperty.intValue, questNames.ToArray());

        //    string selectedQuestName = questNames[selectedQuestIndexProperty.intValue];
        //    QuestSystemQuestSO selectedQuest = AssetDatabase.LoadAssetAtPath<QuestSystemQuestSO>($"{questFolderPath}/{selectedQuestName}.asset");
        //    objectiveProperty.objectReferenceValue = selectedQuest;

        //    EditorGUILayout.Space();
        //}

        //private void StopDrawing(string reason)
        //{
        //    EditorGUILayout.HelpBox(reason, MessageType.Info);
        //    EditorGUILayout.HelpBox("You need to select a Quest for this component to work properly at Runtime!", MessageType.Warning);
        //    serializedObject.ApplyModifiedProperties();
        //}

        //private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldIndex, string oldName, bool isOldPropertyNull)
        //{
        //    if (isOldPropertyNull)
        //    {
        //        indexProperty.intValue = 0;
        //        return;
        //    }

        //    if (oldIndex >= optionNames.Count || oldName != optionNames[oldIndex])
        //    {
        //        indexProperty.intValue = optionNames.Contains(oldName) ? optionNames.IndexOf(oldName) : 0;
        //    }
        //}
    }
}
