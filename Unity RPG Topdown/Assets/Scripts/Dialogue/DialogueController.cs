using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIWindows;
using PlayerShopping;
using QuestSystem;
using Core;

namespace DialogueSystem
{
    using ScriptableObjects;
    using Enumerations;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using InventoryManagement;

    public class DialogueController : Singleton<DialogueController>
    {
        private ResponseHandler responseHandler;

        private DialogueSystemDialogueSO _currentDialogue;
        private DialogueSystemDialogueSO currentDialogue
        {
            get => _currentDialogue;
            set
            {
                _currentDialogue = value;

                if (_currentDialogue != null)
                {
                    // Publish the DialogueStep event to the event bus with the dialogue ID (used by the quest completion tracker)
                    eventBus.Publish("DialogueStep", _currentDialogue.DialogueID);
                }
            }
        }

        private HashSet<Dialogue> activeDialogueComponents = new();
        private Dialogue currentDialogueComponent;

        [SerializeField, FoldoutGroup("Component References")] private TextMeshProUGUI dialogueText;
        [SerializeField, FoldoutGroup("Component References")] private TextMeshProUGUI nameField;
        [SerializeField, FoldoutGroup("Component References")] private GameObject dialogueBox;
        [SerializeField, FoldoutGroup("Component References")] private TypeWriterEffect typeWriterEffect;
        [SerializeField, FoldoutGroup("Component References")] private Transform dialogueBorder;

        [SerializeField] private InventoryManagement.Shop shop;

        private Transform speaker;

        public bool IsOpen { get; private set; }

        private Coroutine currentDialogueCoroutine;

        // Singleton References 
        private EventBus eventBus;
        private QuestCompletionTracker questCompletionTracker;
        private GameStateManager gameStateManager;

        private void Start()
        {
            eventBus = EventBus.Instance;
            questCompletionTracker = QuestCompletionTracker.Instance;
            gameStateManager = GameStateManager.Instance;

            eventBus.Subscribe("CloseMenu", CloseDialogue);

            responseHandler = ResponseHandler.Instance;
            responseHandler.OnPickedResponse += (response) =>
            {
                ShowDialogue(response.NextDialogue);
            };
        }

        public void StartDialogue(Dialogue dialogue, Transform dialogueSpeaker)
        {
            if (IsOpen)
            {
                Debug.LogWarning("Dialogue is already open in StartDialogue!");
                return;
            }

            // Stop all coroutines to ensure no previous coroutine is running
            // Stop the previous coroutine if it exists
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
                currentDialogueCoroutine = null;
            }

            //DialogueOpenedEvent?.Invoke();
            eventBus.Publish("OpenUIMenu");

            speaker = dialogueSpeaker;

            IsOpen = true;

            activeDialogueComponents.Add(dialogue);
            currentDialogueComponent = dialogue;

            // If the dialogue is set to starting dialogues only, then find the first starting dialogue in the group
            if (dialogue.startingDialoguesOnly)
            {
                foreach (var baseNode in dialogue.dialogueContainer.DialogueGroups[dialogue.dialogueGroup])
                {
                    if (baseNode.GetType() == typeof(DialogueSystemDialogueSO))
                    {
                        DialogueSystemDialogueSO dialogueNode = (DialogueSystemDialogueSO)baseNode;

                        if (dialogueNode.IsStartingDialogue)
                        {
                            currentDialogue = dialogueNode;
                        }
                    }
                }
            }
            else
            {
                currentDialogue = dialogue.dialogueContainer.DialogueGroups[dialogue.dialogueGroup][dialogue.selectedDialogueIndex] as DialogueSystemDialogueSO;
            }

            // Before we do anything, we need to determine if any of the condition check nodes have their conditions met
            CheckConditionCheckNodes(dialogue);

            if (currentDialogue == null)
            {
                Debug.Log("Dialogue is not set!");
            }

            nameField.text = dialogueSpeaker.name;

            ReloadDialoguePosition(currentDialogue, speaker);

            dialogueBox.SetActive(true);

            currentDialogueCoroutine = StartCoroutine(StepThroughDialogue(currentDialogue));
        }

        /// <summary>
        /// Find all of the condition check nodes in the dialogue and check if their conditions are met, if so, set the current dialogue to its connected node
        /// </summary>
        /// <param name="dialogue"></param>
        private void CheckConditionCheckNodes(Dialogue dialogue)
        {
            int conditionsMet = 0;

            List<DialogueSystemConditionCheckSO> conditionCheckSOs = new();

            // Retrieve all condition check nodes (Grouped and Ungrouped nodes)
            foreach (var baseNode in dialogue.dialogueContainer.DialogueGroups[dialogue.dialogueGroup])
            {
                if (baseNode.GetType() == typeof(DialogueSystemConditionCheckSO))
                {
                    conditionCheckSOs.Add((DialogueSystemConditionCheckSO)baseNode);
                }
            }

            foreach (var baseNode in dialogue.dialogueContainer.UngroupedDialogues)
            {
                if (baseNode.GetType() == typeof(DialogueSystemConditionCheckSO))
                {
                    conditionCheckSOs.Add((DialogueSystemConditionCheckSO)baseNode);
                }
            }

            Debug.Log($"Iterating through {conditionCheckSOs.Count} condition check nodes");

            // Iterate through the list of retrieved condition check nodes
            foreach (DialogueSystemConditionCheckSO conditionCheckSO in conditionCheckSOs)
            {
                if (conditionCheckSO.ConditionCheckType == ConditionCheckType.CheckGameState)
                {
                    Debug.Log($"Checking condition check node with key {conditionCheckSO.ConditionKey}");

                    if (gameStateManager.GetGameStateBool(conditionCheckSO.ConditionKey) == conditionCheckSO.ExpectedValue)
                    {
                        Debug.Log("Check condition met");

                        // Set the current node to the node that the condition check node is connected to
                        currentDialogue = conditionCheckSO.ConnectedNode as DialogueSystemDialogueSO;

                        conditionsMet++;
                    }
                }
                else if (conditionCheckSO.ConditionCheckType == ConditionCheckType.HasItem)
                {
                    Debug.Log($"Checking condition check node with item id {conditionCheckSO.ItemIDField}");

                    if (string.IsNullOrEmpty(conditionCheckSO.ItemIDField))
                    {
                        Debug.LogWarning("Warning in Dialogue Controller, provided item id to check is not given!");
                        continue;
                    }

                    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                    if (!playerObject.TryGetComponent(out Inventory playerInventory))
                    {
                        Debug.LogWarning("Error removing a player's item in the dialogue controller, player object doesn't' have an inventory");
                        return;
                    }

                    // The player has the item
                    if (playerInventory.ContainsItemByName(conditionCheckSO.ItemIDField))
                    {
                        Debug.Log("Check condition met");

                        currentDialogue = conditionCheckSO.ConnectedNode as DialogueSystemDialogueSO;
                        conditionsMet++;
                    }
                }
            }

            if (conditionsMet > 1)
            {
                Debug.LogWarning("More than one condition check node has been met!");
            }
        }

        public void ShowDialogue(DialogueSystemDialogueSO nextDialogue)
        {
            currentDialogue = nextDialogue;

            //ReloadDialoguePosition(currentDialogue, speaker);
            // ReloadDialoguePosition(currentDialogue, speaker);

            currentDialogueCoroutine = StartCoroutine(StepThroughDialogue(currentDialogue));
        }

        private void ReloadDialoguePosition(DialogueSystemDialogueSO dialogue, Transform speaker)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(speaker.position);

            //dialogueBox.transform.position = screenPosition + new Vector3(0, 90, 0);

            if (dialogue == null)
            {
                throw new System.Exception("Dialogue is null");
            }

            string fullDialogue = string.Join("\n", dialogue.Text);
            dialogueText.text = fullDialogue;

            LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueBox.GetComponent<RectTransform>());
        }

        private IEnumerator StepThroughDialogue(DialogueSystemDialogueSO dialogue)
        {
            if (!IsOpen)
            {
                Debug.LogWarning("Dialogue is not open in StepThroughDialogue!");
                yield break;
            }

            currentDialogue = dialogue;

            // Check the name of the dialogue and if it matches the questCompletionTracker's tracked npc dialouge
            //questCompletionTracker.CheckDialogue(dialogue.DialogueName);

            dialogueBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 100);

            yield return RunTypingEffect(currentDialogue.Text);

            dialogueText.text = currentDialogue.Text;

            float startTime = Time.time;
            float maxWaitTime = 1.2f;

            if (currentDialogue.HasChoices())
            {
                yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space) || (Time.time - startTime >= maxWaitTime));
            }

            foreach (var eventTrigger in currentDialogue.EventTriggers)
            {
                switch (eventTrigger.TriggerType)
                {
                    // Starts a quest if the current dialogue's Dialogue Event is a Quest and the associated object is a quest object
                    case DialogueSystemDiagEvent.StartQuest:
                        if (eventTrigger.TriggerValue is Quest)
                        {
                            // Get the quest from the associated object
                            Quest quest = eventTrigger.TriggerValue as Quest;
                            questCompletionTracker.AddQuest(quest);
                        }

                        break;

                    case DialogueSystemDiagEvent.ShopOpen:
                        if (eventTrigger.TriggerValue is ShopData)
                        {
                            shop.OpenShop(eventTrigger.TriggerValue as ShopData);
                            CloseDialogue(false);
                            yield break;
                        }
                        else
                        {
                            throw new System.Exception($"Associated object to dialogue {currentDialogue.name} is not a shop!");
                        }

                    // Changes the dialogue's group if the current dialogue's Dialogue Event is a Set Group
                    case DialogueSystemDiagEvent.SetGroup:

                        if (currentDialogueComponent.GetGroupOfName(eventTrigger.TriggerValue as string) == null)
                        {
                            throw new System.Exception("The next dialogue group doesn't exist!");
                        }

                        currentDialogueComponent.dialogueGroup = currentDialogueComponent.GetGroupOfName(eventTrigger.TriggerValue as string);

                        break;

                    // Removes the specified item from the player's inventory
                    case DialogueSystemDiagEvent.RemovePlayerItem:

                        string itemId = eventTrigger.TriggerValue as string;

                        if (string.IsNullOrEmpty(itemId))
                        {
                            Debug.LogWarning("Error removing a player's item in the dialogue controller, the provided item id is null or empty");
                            break;
                        }

                        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                        if (!playerObject.TryGetComponent(out Inventory playerInventory))
                        {
                            Debug.LogWarning("Error removing a player's item in the dialogue controller, player object doesn't' have an inventory");
                        }

                        if (!playerInventory.ContainsItemByName(itemId))
                        {
                            Debug.LogWarning("Error removing a player's item in the dialogue controller, the item doesn't exist in the player's inventory");
                        }

                        // Remove
                        playerInventory.RemoveItemByID(itemId);

                        break;
                }
            }

            yield return new WaitForEndOfFrame();

            // If the current dialogue has choices, show the choices
            if (currentDialogue.HasChoices())
            {
                responseHandler.ShowResponses(currentDialogue.Choices);
                yield break;
            }

            // If the current dialogue has no choices, close the dialogue 
            else if (currentDialogue.Choices.Count == 1 && currentDialogue.Choices[0].NextDialogue == null)
            {
                yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
                CloseDialogue();
                yield break;
            }

            // If the current dialogue has no choices, show the next dialogue
            else
            {
                yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
                ShowDialogue(currentDialogue.Choices[0].NextDialogue);
                yield break;
            }
        }

        private IEnumerator RunTypingEffect(string dialogue)
        {
            typeWriterEffect.Run(dialogue, dialogueText);

            while (typeWriterEffect.IsRunning)
            {
                yield return null;

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    typeWriterEffect.Stop();
                    break;
                }
            }
        }

        private void CloseDialogue() => CloseDialogue(true);
        private void CloseDialogue(bool invokeDialogueClose = true)
        {
            if (!IsOpen)
                return;

            if (invokeDialogueClose)
            {
                eventBus.Publish("CloseUIMenu");
            }

            // Reset the IsOpen flag
            IsOpen = false;

            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;

            dialogueBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 100);
            dialogueText.text = string.Empty;

            foreach (Transform child in dialogueBorder)
            {
                Destroy(child.gameObject);
            }

            currentDialogue = null;
            typeWriterEffect.Stop();
            dialogueBox.SetActive(false);
        }
    }
}
