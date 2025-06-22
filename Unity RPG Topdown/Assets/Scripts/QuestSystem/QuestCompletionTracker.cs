using UnityEngine;
using System.Collections.Generic;
using Characters;
using Core;
using Items;
using NPCManagement;

namespace QuestSystem
{
    using Data.Save;
    using Enumerations;
    using InventoryManagement;
    using QuestSystem.ScriptableObjects;
    using System;
    using System.Linq;

    public class QuestCompletionTracker : Singleton<QuestCompletionTracker>
    {
        private HashSet<OngoingQuest> ongoingQuests;

        // Singleton References
        private EventBus eventBus;
        private NPCDirectory npcDirectory;
        private ItemDatabase itemDatabase;
       // private QuestConditionsUI questConditionsUI;

        [SerializeField] private Inventory playerInventory;

        // Actions
        public Action<OngoingQuest> OnQuestAdd;
        public Action OnQuestConditionChanged;

        // Debug
        [SerializeField] private List<Quest> testQuests;

        private void Start()
        {
           // questConditionsUI = QuestConditionsUI.Instance;
            eventBus = EventBus.Instance;
            itemDatabase = ItemDatabase.Instance;
            npcDirectory = NPCDirectory.Instance;

            ongoingQuests = new();

            eventBus.Subscribe<string>("DialogueStep", OnDialogueStep);
        }

        /// <summary>
        /// Triggered when we activate a dialogue node. Check if any of the ongoing quests have a condition that matches the dialogue
        /// </summary>
        /// <param name="dialogueID"></param>
        private void OnDialogueStep(string dialogueID)
        {
            // Loop through each of the ongoing quests
            foreach (OngoingQuest ongoingQuest in ongoingQuests)
            {
                // Go through all of the conditions in the quest, if any have the same dialogueID, mark them as complete
                if (ongoingQuest.ContainConditionVariable(dialogueID))
                {
                    // Sets the condition to true
                    ongoingQuest.MarkCondition(dialogueID, true);

                    OnQuestConditionChanged?.Invoke();
                    Debug.Log("Dialogue Match!");
                }
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
            {
                foreach (Quest testQuest in testQuests)
                {
                    AddQuest(testQuest);
                }
            }

            TrackQuestConditions();
        }

        public void AddQuest(Quest questToStart)
        {
            if (QuestExists(questToStart))
            {
                Debug.LogError("Quest already exists!");
                return;
            }
            else
            {
                Debug.Log($"Adding Quest: {questToStart.QuestName}");
            }

            // Create a new OngoingQuest object and add it to the ongoingQuests list
            OngoingQuest newOngoingQuest = new(questToStart);

            // Add the new quest to the list
            ongoingQuests.Add(newOngoingQuest);

            // Get the starting quest from the Quest object
            questToStart.GetStartingQuest();

            // Tell the UI to add the new quest
            OnQuestAdd?.Invoke(newOngoingQuest);

            // Setup this quest for tracking
            SetupQuest(newOngoingQuest);
        }

        /// <summary>
        /// Iterate through each of the added quests and check if their conditions are met
        /// </summary>
        private void TrackQuestConditions()
        {
            foreach (OngoingQuest ongoingQuest in ongoingQuests.ToList())
            {
                Quest quest = ongoingQuest.quest;

                foreach (QuestSystemQuestConditionData condition in quest.objective.Conditions)
                {
                    switch (condition.ConditionType)
                    {
                        case QuestCondition.PressKey:

                            // The Key is in string format, so we need to convert it to KeyCode
                            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), condition.ConditionValue.ToUpper());

                            if (key == KeyCode.None)
                            {
                                Debug.LogError("Invalid KeyCode");
                            }

                            if (Input.GetKeyDown(key))
                            {
                                ongoingQuest.MarkCondition(condition.ConditionValue, true);
                                OnQuestConditionChanged?.Invoke();
                               // questConditionsUI.CompleteQuestPoint(questText);
                            }

                            break;

                        case QuestCondition.CollectItem:

                            // Check if the player has the given item in their inventory
                            if (playerInventory.ContainsItemByName(condition.ConditionValue))
                            {
                                string questText = GetQuestObjectiveDescription(condition);

                                ongoingQuest.MarkCondition(condition.ConditionValue, true);
                                OnQuestConditionChanged?.Invoke();

                                //questConditionsUI.CompleteQuestPoint(questText);
                            }

                            // This condition might be 'disabled', so we need to re-update
                            else
                            {
                                ongoingQuest.MarkCondition(condition.ConditionValue, false);
                                OnQuestConditionChanged?.Invoke();

                                // questConditionsUI.SetQuestPointUncomplete(questText);
                            }

                            break;
                    }
                }

                // If the quest step is complete, move to the next step
                if (ongoingQuest.IsObjectiveComplete())
                {
                    quest.IncreaseQuestStep();

                    SetupQuest(ongoingQuest);

                    OnQuestConditionChanged?.Invoke();

                    Debug.Log("Quest Step Complete!");
                }
            }
        }

        /// <summary>
        /// If any of the currently tracked quests require a dialogue node to be triggered, check if the given dialogue matches with any of the quest conditions
        /// </summary>
        /// <param name="dialogueName"></param>
        public void CheckDialogue(string dialogueName)
        {
            foreach (OngoingQuest ongoingQuest in ongoingQuests)
            {
                foreach (QuestSystemQuestConditionData condition in ongoingQuest.quest.objective.Conditions)
                {
                    if (condition.ConditionType == QuestCondition.ActivateNPCDialogueStep)
                    {
                        if (condition.ConditionValue == dialogueName)
                        {
                            Debug.Log("Dialogue Match!");

                            ongoingQuest.MarkCondition(condition.ConditionValue, true);

                            OnQuestConditionChanged?.Invoke();

                            //questConditionsUI.CompleteQuestPoint(questText);
                        }
                    }
                }
            }
        }

        private void SetupQuest(OngoingQuest ongoingQuest)
        {
            // Make a new dictionary to track the quest conditions
            ongoingQuest.ReInitialize();

            foreach (QuestSystemQuestConditionData condition in ongoingQuest.quest.objective.Conditions)
            {
                //Debug.Log($"Adding Quest Condition: {condition.ConditionType} with value {condition.ConditionValue}");

                string questText = GetQuestObjectiveDescription(condition);

                QuestConditionData newConditionData = new()
                {                  
                    ConditionVariable = condition.ConditionValue,
                    ConditionDisplayName = questText,
                    CompletionStatus = false
                };

                ongoingQuest.AddQuestCondition(newConditionData);
            }

            // Subscribe to the quest's completion event
            ongoingQuest.quest.OnQuestComplete += () =>
            {
                ongoingQuests.Remove(ongoingQuest);
                OnQuestConditionChanged?.Invoke();
            };

            OnQuestConditionChanged?.Invoke();

            DoQuestTriggers(ongoingQuest.quest.objective);
        }

        private string GetQuestObjectiveDescription(QuestSystemQuestConditionData condition)    
        {
            switch (condition.ConditionType)
            {
                case QuestCondition.PressKey:
                    return $"Press Key: {condition.ConditionValue}";

                case QuestCondition.KillTarget:

                    // We need to check if the NPC exists in the directory
                    NPC npc = npcDirectory.GetLivingNPCWithID(condition.ConditionValue);
                    if (npc != null)
                    {
                        return $"Kill Target: {npc.NPCName}";
                    }
                    else
                    {
                        Debug.LogError("NPC not found in directory");
                        return $"Error, NPC not found!";
                    }

                case QuestCondition.CollectItem:

                    // We need to check if the item exists in the database
                    string itemName = itemDatabase.GetItemNameFromID(condition.ConditionValue);

                    if (string.IsNullOrEmpty(itemName))
                    {
                        Debug.LogError("Item not found in database");   
                        return $"Error, Item not found!";
                    }

                    return $"Collect Item: {itemName}";

                case QuestCondition.ActivateNPCDialogueStep:

                    if (string.IsNullOrEmpty(condition.ConditionValue))
                    {
                        Debug.LogError("Invalid NPC Dialogue Step");
                        return $"Error, NPC Dialogue Step not found!";
                    }

                    return $"Talk to NPC";

                default:
                    break;
            }

            return "Error, Condition not found!";
        }

        private bool QuestExists(Quest quest)
        {
            foreach (OngoingQuest ongoingQuest in ongoingQuests)
            {
                if (ongoingQuest.quest.QuestName == quest.QuestName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Takes any triggers from a quest node and applies them to the game state manager
        /// </summary>
        private void DoQuestTriggers(QuestSystemQuestSO questNode)
        {
            if (questNode.Triggers == null && questNode.Triggers.Count == 0)
                return;

            foreach(QuestSystemQuestTriggerData trigger in questNode.Triggers)
            {
                GameStateManager.Instance.SetGameState(trigger.ConditionKey, trigger.ConditionValue);
            }
        }
    }
}