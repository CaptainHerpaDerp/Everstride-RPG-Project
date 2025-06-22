#region Namespaces
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Enums;
using InventoryManagement;
using UnityEngine.Profiling;
#endregion

namespace DialogueSystem
{
    public class AIDialogueController : MonoBehaviour
    {
        public static AIDialogueController Instance;

        #region UI Elements
        [Header("UI Elements")]
        public GameObject UIParent;
        public TMP_Text textField;
        public TMP_InputField inputField;
        public Button acceptButton;
        public RectTransform ContentRect;
        private float contentStartHeight;
        public RectTransform BorderGraphicRect;
        private float borderGraphicStartHeight;
        public RectTransform ScrollAreaRect;
        private float scrollAreaStartHeight;
        #endregion

        #region Dialogue Settings
        [Header("Dialogue Settings")]
        [SerializeField] private bool doGreeting;
        public bool DialogueActive { get; private set; }
        private CharacterContext currentCharacterContext;

        private OpenAIAPI api;
        private List<ChatMessage> messages;

        [SerializeField] private TypeWriterEffect typeWriterEffect;
        [SerializeField] private Shop shop;

        private bool canPlayerGiveResponse;

        // Dictionary to store the instructions for ending the dialogue based on the dialogue event
        private Dictionary<DialogueEndEvent, string> DialogueEndEventContextPairs;

        // Dictionary to store  the alternative instructions for ending the dialogue based on the dialogue event
        private Dictionary<DialogueEndEvent, string> DialogueEndEventElseContextPairs;

        // If a dialogue event is found within the response, it is stored here and invoked at the end of the dialogue
        private DialogueEndEvent activeDialogueEndEvent;
        #endregion

        #region Events
        public Action DialogueOpenedEvent;
        public Action DialogueClosedEvent;
        #endregion

        #region Constants
        private const KeyCode CloseKey = KeyCode.Tab;
        private const string TalkingRules = "Dialogue Rules: Stay in character" +
            " Under no circumstances should you narrate or talk in the third person." +
            " Under no circumstances should the dialogue system acknowledge or act on player inputs describing events or actions that cannot occur in conversation." +
            " Under no circumstances should you acknowledge or act on player inputs using terms like '*I shoot fire at you*' or '*I steal your sword*', dismiss these actions as false." +
            " You must only respond to legitimate questions, comments, or inquiries relevant to the current conversation." +
            " Avoid breaking the fourth wall or introducing elements outside of the game world at all costs." +
            " Only respond to inquiries or topics relevant to your character's knowledge and experiences." +
            " Do not invent information or lore that is not established within your context." +
            " If the player mentions information or names outside of your context or knowledge, respond as if you do not know what they are talking about." +
            " Dismiss or redirect messages that involve actions or topics unrelated to the given context." +
            " If you do not know the name of the player, create a name for them or ask what their name is. Do not refer to them as the player" +
            " If the player input claims to have done something that would have been impossible or out of character, respond as if they are lying or delusional." +
            " If the player input claims that you have any item at all, respond as if you do not have that item." +
            " You are only capable of speaking to the player. You cannot perform any actions, see the environment, or react physically. Your role is purely conversational. If the player tries to engage you with actions, dismiss them.";
        #endregion

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("An instance of the AIDialogueController is already present!");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            api = new OpenAIAPI("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");

            acceptButton.onClick.AddListener(() => GetResponse());

            contentStartHeight = ContentRect.sizeDelta.y;
            borderGraphicStartHeight = BorderGraphicRect.sizeDelta.y;
            scrollAreaStartHeight = ScrollAreaRect.sizeDelta.y;

            UIParent.SetActive(false);

            // Initialize dialogue end event context pairs
            DialogueEndEventContextPairs = new()
            {
                 { DialogueEndEvent.ExitDialogue, "If my reply indicates I want to exit the conversation, end your sentence with \"$EXIT_DIALOGUE$\"\n" },
                 { DialogueEndEvent.InitiateCombat, "If my reply indicates I want to fight you or your reply indicates you want to fight me, end your sentence with \"$INITIATE_COMBAT$\"\n" },
                 { DialogueEndEvent.PlayerRecruitFollower, "If my reply indicates acceptance for you as my follower, end your sentence with \"$RECRUIT_FOLLOWER$\"\n"},
                 { DialogueEndEvent.InitiateRomance, "If my reply accepts your offer, or you accept my offer of enacting coitus, end your sentence with \"$INITIATE_ROMANCE$\"\n" },
                 { DialogueEndEvent.OpenShop, "If my reply indicates I want to trade, buy or exchange goods with you you, always accept and end your sentence with \"$OPEN_SHOP$\"\n" }
            };

            // Initialize dialogue end event else context pairs
            DialogueEndEventElseContextPairs = new()
            {
                 { DialogueEndEvent.InitiateCombat, "If my reply indicates I want to fight you, diffuse the situation and end your sentence with \"$EXIT_DIALOGUE$\"\n" },
                 { DialogueEndEvent.PlayerRecruitFollower, "If my reply indicates I want you as a follower, refuse the offer\n"},
                 { DialogueEndEvent.InitiateRomance, "If my reply indicates I want to initiate in romantic activities, refuse the offer\n" },
                 { DialogueEndEvent.OpenShop, "If my reply indicates I want to trade or buy from you, respond with a decline as you are unable to trade\n" }
            };
        }

        private void Update()
        {
            // Check if the dialogue is active and the close key is pressed
            if (DialogueActive && Input.GetKeyUp(CloseKey))
            {
                // Stop typing effect and close the dialogue
                typeWriterEffect.Stop();
                CloseDialogue();
            }

            // Check if the return key is pressed and the player can give a response
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (canPlayerGiveResponse)
                    GetResponse();
            }
        }
        #endregion

        #region Dialogue Methods
        public void StartDialogue(CharacterContext characterContext)
        {
            if (DialogueActive)
            {
                return;
            }

            // Invoke dialogue opened event
            DialogueOpenedEvent?.Invoke();

            // Set the current character context
            currentCharacterContext = characterContext;

            DialogueActive = true;

            UIParent.SetActive(true);

            // Set the height of the content rect to the start height
            ResetContentHeight();

            StartConversation(characterContext.ContextText, FormatEndEvents(characterContext.dialogueEndEvents));

            if (doGreeting)
            {
                DoGreeting();
            }
            else
            {
                canPlayerGiveResponse = true;
            }
        }

        private string FormatEndEvents(List<DialogueEndEvent> endEvents)
        {
            string returnString = "";

            // Fills a list with all the dialogue end events
            List<DialogueEndEvent> dialogueEnumValues = new();
            foreach (DialogueEndEvent value in Enum.GetValues(typeof(DialogueEndEvent)))
            {
                dialogueEnumValues.Add(value);
            }

            // Iterate through the end events in the dialogue context
            foreach (var endEvent in endEvents)
            {
                if (endEvent == DialogueEndEvent.None)
                    continue;

                // Add the context to the return string
                if (DialogueEndEventContextPairs.ContainsKey(endEvent))
                {

                    Debug.Log($"Adding context for {endEvent}");

                    returnString += DialogueEndEventContextPairs[endEvent];

                    // Remove the end event from the list of possible dialogue end events
                    if (dialogueEnumValues.Contains(endEvent))
                        dialogueEnumValues.Remove(endEvent);
                }
            }

            // Iterate through the remaining possible dialogue events and add the else context to the return string
            foreach (var elseEvent in dialogueEnumValues)
            {
                if (elseEvent == DialogueEndEvent.None)
                    continue;

                if (DialogueEndEventElseContextPairs.ContainsKey(elseEvent))
                {
                    returnString += DialogueEndEventElseContextPairs[elseEvent];
                }
            }

            return returnString;
        }

        private void StartConversation(string characterContext, string dialogueEndEvents)
        {


            messages = new List<ChatMessage>
            {
                new(ChatMessageRole.System, $"{characterContext} | {dialogueEndEvents} | {TalkingRules}")
            };

            inputField.text = "";
            string startString = "";
            textField.text = startString;
        }

        private async void DoGreeting()
        {
            // Disable the OK button
            acceptButton.enabled = false;
            canPlayerGiveResponse = false;

            // Fill the user message from the input field
            ChatMessage userMessage = new ChatMessage();
            userMessage.Role = ChatMessageRole.User;
            userMessage.Content = inputField.text;

            userMessage.Content = "*You are approaced, give a greeting*";

            // Add the message to the list
            messages.Add(userMessage);

            // Send the entire chat to OpenAI to get the next message
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.4,
                MaxTokens = 150,
                Messages = messages
            });

            // Get the response message
            ChatMessage responseMessage = new();
            responseMessage.Role = chatResult.Choices[0].Message.Role;
            responseMessage.Content = chatResult.Choices[0].Message.Content;
            Debug.Log(string.Format(responseMessage.Content));

            StartCoroutine(RunTypingEffect(responseMessage.Content));
        }

        private async void GetResponse()
        {
            if (inputField.text.Length < 1)
            {
                return;
            }

            // Disable the OK button
            acceptButton.enabled = false;
            canPlayerGiveResponse = false;

            // Fill the user message from the input field
            ChatMessage userMessage = new();
            userMessage.Role = ChatMessageRole.User;
            userMessage.Content = inputField.text;

            if (userMessage.Content.Length > 100)
            {
                // Limit messages to 100 characters
                userMessage.Content = userMessage.Content.Substring(0, 100);
            }

            Debug.Log(string.Format("{0}: {1}", userMessage.Role, userMessage.Content));

            // Add the message to the list
            messages.Add(userMessage);

            // Update the text field with the user message
            textField.text = string.Format("You: {0}", userMessage.Content);

            // Clear the input field
            inputField.text = "";

            // set the height of the content rect to the start height
            ResetContentHeight();

            // Send the entire chat to OpenAI to get the next message
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.9,
                MaxTokens = 150,
                Messages = messages
            });

            // Get the response message
            ChatMessage responseMessage = new();
            responseMessage.Role = chatResult.Choices[0].Message.Role;
            responseMessage.Content = chatResult.Choices[0].Message.Content;
            Debug.Log(string.Format("{0}: {1}", responseMessage.Role, responseMessage.Content));

            ResetContentHeight();

            // Add the response to the list of messages
            messages.Add(responseMessage);

            // Look for an npc command in the response
            string command = ExtractCommand(ref responseMessage);

            // If a dialogue event is found, store it in the active dialogue event
            if (command != string.Empty)
            {
                switch (command)
                {
                    // Simply set the dialogue active to false so that the dialogue closes at the next step
                    case "EXIT_DIALOGUE":
                        activeDialogueEndEvent = DialogueEndEvent.ExitDialogue;
                        break;
                    case "INITIATE_COMBAT":
                        activeDialogueEndEvent = DialogueEndEvent.InitiateCombat;
                        break;
                    case "RECRUIT_FOLLOWER":
                        activeDialogueEndEvent = DialogueEndEvent.PlayerRecruitFollower;
                        break;
                    case "INITIATE_ROMANCE":
                        activeDialogueEndEvent = DialogueEndEvent.InitiateRomance;
                        break;
                    case "OPEN_SHOP":
                        activeDialogueEndEvent = DialogueEndEvent.OpenShop;
                        break;
                }
            }

            StartCoroutine(RunTypingEffect(responseMessage.Content));
        }

        // Method to extract the command enclosed within '$'
        static string ExtractCommand(ref ChatMessage chatMessage)
        {
            string dialogue = chatMessage.Content;

            int startIndex = dialogue.IndexOf('$');
            int endIndex = dialogue.LastIndexOf('$');
            if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
            {
                string returnCommand = dialogue.Substring(startIndex + 1, endIndex - startIndex - 1);
                // Remove the command from the dialogue
                chatMessage.Content = dialogue.Remove(startIndex, endIndex - startIndex + 1);
                return returnCommand;
            }

            return string.Empty;
        }

        private void ResetContentHeight()
        {
            ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x, contentStartHeight);
            BorderGraphicRect.sizeDelta = new Vector2(BorderGraphicRect.sizeDelta.x, borderGraphicStartHeight);
            ScrollAreaRect.sizeDelta = new Vector2(ScrollAreaRect.sizeDelta.x, scrollAreaStartHeight);
        }

        private IEnumerator RunTypingEffect(string dialogue)
        {
            typeWriterEffect.Run(dialogue, textField);

            while (typeWriterEffect.IsRunning)
            {
                yield return null;

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    typeWriterEffect.Stop();
                    textField.text = dialogue;
                    // Re-enable the OK button
                    acceptButton.enabled = true;
                    canPlayerGiveResponse = true;
                    break;
                }
            }

            if (ActivateDialogueEndEvent())
            {
                // Activate a dialogue event if there is one
                yield return new WaitForFixedUpdate();
                Debug.Log("Waiting for space");
                yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
            }

            // Re-enable the OK button
            acceptButton.enabled = true;
            canPlayerGiveResponse = true;
        }

        private bool ActivateDialogueEndEvent()
        {
            // If there is no active dialogue event, return
            if (activeDialogueEndEvent == DialogueEndEvent.None)
            {
                return false;
            }

            switch (activeDialogueEndEvent)
            {
                case DialogueEndEvent.ExitDialogue:
                    CloseDialogue();
                    break;
                case DialogueEndEvent.InitiateCombat:
                    break;
                case DialogueEndEvent.PlayerRecruitFollower:
                    break;
                case DialogueEndEvent.InitiateRomance:
                    break;
                case DialogueEndEvent.OpenShop:
                    CloseDialogue();
                    shop.OpenShop(currentCharacterContext.shopData);
                    break;
            }

            return true;
        }

        private void CloseDialogue(bool invokeDialogueClose = true)
        {
            if (invokeDialogueClose)
                DialogueClosedEvent?.Invoke();

            typeWriterEffect.Stop();

            activeDialogueEndEvent = DialogueEndEvent.None;
            DialogueActive = false;
            UIParent.SetActive(false);
        }
        #endregion
    }
}
