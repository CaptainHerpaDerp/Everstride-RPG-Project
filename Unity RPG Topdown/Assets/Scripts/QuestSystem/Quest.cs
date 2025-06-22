using Core;
using QuestSystem.Data.Save;
using QuestSystem.ScriptableObjects;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
    public class Quest : ScriptableObject
    {
        [Title("Quest Settings")]
        [InlineEditor(InlineEditorModes.FullEditor)]
        [Tooltip("The container that holds all quests and groups.")]
        public QuestSystemQuestContainerSO questContainer;

        [ShowIf("@questContainer != null")]
        [Tooltip("The group this quest belongs to (if any).")]
        public QuestSystemQuestGroupSO questGroup;

        [ShowIf("@questContainer != null")]
        [Tooltip("The quest objective.")]
        public QuestSystemQuestSO objective;

        [Title("Filters")]
        [Tooltip("Show grouped quests only.")]
        public bool groupedQuests;

        [Tooltip("Show starting quests only.")]
        public bool startingQuestsOnly;

        [FoldoutGroup("Advanced Settings")]
        [ReadOnly, Tooltip("The current index of the selected quest group.")]
        public int selectedQuestGroupIndex;

        [FoldoutGroup("Advanced Settings")]
        [ReadOnly, Tooltip("The current index of the selected quest.")]
        public int selectedQuestIndex;

        [Title("Runtime Information")]
        [ReadOnly, Tooltip("The current state of this quest.")]
        public string currentState;

        [field: SerializeField] public string QuestName { get; private set; }

        public Action OnQuestComplete;

        public QuestSystemQuestSO GetStartingQuest()
        {
            // Method 1
            //questGroup = questContainer.QuestGroups.Keys.ToList()[selectedQuestGroupIndex];

            // Method 2
            questGroup = questContainer.QuestGroups.ElementAt(selectedQuestGroupIndex).Key;


            if (startingQuestsOnly)
            {
                foreach (var node in questContainer.QuestGroups[questGroup])
                {
                    QuestSystemQuestSO questNode = node as QuestSystemQuestSO;

                    if (questNode == null)
                    {
                        Debug.Log($"Cast failed in {name}");
                        return null;
                    }

                    if (questNode.IsRootNode)
                    {
                        objective = questNode;
                    }
                }
            }
            else
            {
                objective = questContainer.QuestGroups[questGroup][selectedQuestIndex] as QuestSystemQuestSO;
            }

            if (objective != null)
            {

                return objective;
            }
            else
            {
                Debug.LogError("Couldn't find the starting quest!");
                return null;
            }
        }

        public void IncreaseQuestStep()
        {
            if (objective.NextNodeID == "")
            {
                Debug.Log("No more nodes, quest is complete!");
                OnQuestComplete?.Invoke();
                return;
            }

            objective = GetQuestFromID(objective.NextNodeID);

            if (objective == null)
            {
                Debug.LogError("Couldn't find the next quest node!");
            }
            else
            {
                // We need to iterate through the triggers of the objective and add them to the Game State Manager
                if (objective.Triggers != null && objective.Triggers.Count > 0)
                {
                    foreach (var trigger in objective.Triggers)
                    {
                        GameStateManager.Instance.SetGameState(trigger.ConditionKey, trigger.ConditionValue);
                    }
                }
            }
        }

        private QuestSystemQuestSO GetQuestFromID(string nodeId)
        {
            foreach (var questGroup in questContainer.QuestGroups)
            {
                foreach (var quest in questGroup.Value)
                {
                    QuestSystemQuestSO questNode = quest as QuestSystemQuestSO;

                    if (questNode == null)
                    {
                        Debug.Log($"Cast failed in {name}");
                        return null;
                    }

                    if (questNode.NodeID == nodeId)
                    {                        
                        return questNode;
                    }
                }
            }

            return null;
        }

        public void SetQuestContainer(QuestSystemQuestContainerSO questContainer)
        {
            this.questContainer = questContainer;
        }

        public QuestSystemQuestGroupSO GetGroupOfName(string name)
        {
            foreach (var item in questContainer.QuestGroups)
            {
                if (item.Key.name == name)
                {
                    return item.Key;
                }
            }

            throw new System.Exception("Quest Group not found");
        }

        public string GetCurrentQuestGroupName()
        {
            return questGroup.name;
        }

        public void SetCurrentQuestGroup(string newGroup)
        {
            questGroup = GetGroupOfName(newGroup);
        }

        public void UpdateCurrentQuestGroup(string npcID, string questGroupName)
        {
            throw new System.NotImplementedException();
        }
    }
}