using UnityEngine;
using Core;
using QuestSystem;
using TMPro;
using UIElements;
using Sirenix.OdinInspector;
using InventoryManagement;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace UIWindows
{

    /// <summary>
    /// Displays active quests in the player menu
    /// </summary>
    public class QuestMenu : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private CanvasGroup canvasGroup;

        [FoldoutGroup("Quest Bars"), Header("The bar that will be instantiated when a new quest is added")]
        [SerializeField] private QuestBar questBarPrefab;

        [FoldoutGroup("Quest Bars"), Header("Where the tracked quest bar will be parented")]
        [SerializeField] private Transform trackedQuestBarParentTransform;

        [FoldoutGroup("Quest Bars"), Header("Where the rest of the quest bars will be parented")]
        [SerializeField] private Transform questBarParentTransform;

        [FoldoutGroup("Quest Information"), SerializeField] private TextMeshProUGUI questNameTextComponent;
        [FoldoutGroup("Quest Information"), SerializeField] private TextMeshProUGUI questDescriptionTextComponent;
        [FoldoutGroup("Quest Information"), SerializeField] private Transform questObjectivesParentTransform;
        [FoldoutGroup("Quest Information"), SerializeField] private QuestObjective questObjectivePrefab;

        // A dictionary to store each created quest bar with the name of the quest as the key
        private Dictionary<string, QuestBar> questNameBarPairs = new();

        private string questNameText
        {
            get => questNameTextComponent.text;
            set => questNameTextComponent.text = value;
        }

        private string questDescriptionText
        {
            get => questDescriptionTextComponent.text;
            set => questDescriptionTextComponent.text = value;
        }

        // The tracked quest with all the quest conditions and other info that will be displayed in the quest menu
        private OngoingQuest _trackedQuest;

        private OngoingQuest trackedQuest
        {
            get => _trackedQuest;

            set
            {
                _trackedQuest = value;

                // When a new quest is tracked, update the quest information
                ReloadQuestInformation();
            }
        }

        // Singleton References
        private QuestCompletionTracker questCompletionTracker;
        private EventBus eventBus;
        private InventoryMenu inventoryMenu;

        private void Start()
        {
            questCompletionTracker = QuestCompletionTracker.Instance;
            eventBus = EventBus.Instance;
            inventoryMenu = InventoryMenu.Instance;

            SubscribeToEvents();
        }

        /// <summary>
        /// Subscribe to the events present in the quest completion tracker
        /// </summary>
        private void SubscribeToEvents()
        {
            questCompletionTracker.OnQuestAdd += AddQuest;
            questCompletionTracker.OnQuestConditionChanged += ReloadQuestInformation;

            inventoryMenu.OnQuestMenuOpen += OpenMenu;
            inventoryMenu.OnQuestMenuClosed += CloseMenu;
        }

        public void OpenMenu()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void CloseMenu()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Adds a quest bar to the quest menu and if there isn't a quest being tracked, set the tracked quest to the new quest
        /// </summary>
        /// <param name="newOngoingQuest"></param>
        private void AddQuest(OngoingQuest newOngoingQuest)
        {
            QuestBar newQuestBar = Instantiate(questBarPrefab, parent: questBarParentTransform);

            // Set the name of the quest in the quest bar
            newQuestBar.QuestName = newOngoingQuest.quest.QuestName;

            // Add the quest bar to the dictionary with the quest name as the key
            questNameBarPairs.Add(newOngoingQuest.quest.QuestName, newQuestBar);

            // If the isn't a quest being tracked when a quest is added, set the tracked quest to the new quest
            if (trackedQuest == null)
            {
                trackedQuest = newOngoingQuest;
                SetQuestBarTracked(newQuestBar);
            }

            // Subscribe to the quest bar button click event, if it is pressed, we want to track the quest
            newQuestBar.Button.onClick.AddListener(() =>
            {
                Debug.Log("Quest Bar Clicked: " + newOngoingQuest.quest.QuestName);
                trackedQuest = newOngoingQuest;

                // Set the quest bar to be tracked
                SetQuestBarTracked(newQuestBar);
            });

            // Remove the quest when it is completed
            newOngoingQuest.quest.OnQuestComplete += () =>
            {
                // Remove the quest bar from active quest list
                Destroy(newQuestBar.gameObject);

                // Remove any other references to the quest
                RemoveQuest(newOngoingQuest);
            };
        }

        private void ReloadQuestInformation()
        {
            // If the quest is null, simply remove all elements from the quest menu
            if (trackedQuest == null)
            {
                questNameText = "";

                questDescriptionText = "";

                foreach (Transform child in questObjectivesParentTransform)
                {
                    Destroy(child.gameObject);
                }
            }

            else
            {
                // Set the quest name text to the name of the tracked quest
                questNameText = trackedQuest.quest.QuestName;

                // Set the quest description text to the description of the current objective of the tracked quest
                questDescriptionText = trackedQuest.quest.objective.Description;

                // Clear the quest objectives parent transform of any children
                foreach (Transform child in questObjectivesParentTransform)
                {
                    Destroy(child.gameObject);
                }

                // Iterate through each of the conditions of the tracked quest and create a new quest objective for each
                foreach (var condition in trackedQuest.QuestConditions)
                {
                    QuestObjective newQuestObjective = Instantiate(questObjectivePrefab, parent: questObjectivesParentTransform);

                    string questName = trackedQuest.quest.QuestName;

                    newQuestObjective.ObjectiveText = condition.ConditionDisplayName;
                    newQuestObjective.SetObjectiveComplete(condition.CompletionStatus);
                }
            }
        }

        /// <summary>
        /// Sets a quest bar as being tracked visually, and sets its child index to be the first child
        /// </summary>
        /// <param name="questBar"></param>
        private void SetQuestBarTracked(QuestBar questBar)
        {
            // First, disable the tracked state of all quest bars
            foreach (var pair in questNameBarPairs)
            {
                // Mark the bar as not being tracked
                pair.Value.SetQuestTracked(false);

                // Set the parent of the quest bar to be the untracked quest bar parent transform
                pair.Value.transform.SetParent(questBarParentTransform);
            }

            // Set the quest bar to be tracked
            questBar.SetQuestTracked(true);

            // Set the quest bar to be a child of the tracked quest bar parent transform
            questBar.transform.SetParent(trackedQuestBarParentTransform);
        }

        /// <summary>
        /// Removes a quest from the quest menu
        /// </summary>
        /// <param name="questToRemove"></param>
        private void RemoveQuest(OngoingQuest questToRemove)
        {
            // If the quest to remove is the tracked quest, set the tracked quest to null
            if (questToRemove == trackedQuest)
            {
                trackedQuest = null;
                ReloadQuestInformation();
            }

            // Remove the quest bar from the dictionary
            questNameBarPairs.Remove(questToRemove.quest.QuestName);
        }
    }
}