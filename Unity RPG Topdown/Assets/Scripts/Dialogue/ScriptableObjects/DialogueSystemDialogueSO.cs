using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.Data;
using DialogueSystem.Enumerations;
using GraphSystem.Base.ScriptableObjects;
using DialogueSystem.Data.Save;

namespace DialogueSystem.ScriptableObjects
{

    public class DialogueSystemDialogueSO : BaseNodeSO
    {
        [field: SerializeField] public string DialogueID { get; set; }
        [field: SerializeField][field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DialogueSystemDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueSystemDiagType DialogueType { get; set; }
        [field: SerializeField] public List<DialogueSystemEventTriggerSaveData> EventTriggers { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialize(string ID, string text, List<DialogueSystemDialogueChoiceData> choices, 
            DialogueSystemDiagType dialogueType, bool isStartingDialogue, List<DialogueSystemEventTriggerSaveData> eventTriggers)
        {
            DialogueID = ID;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
            EventTriggers = eventTriggers;
        }


        public bool HasChoices()
        {
            return Choices != null && Choices.Count > 1;
        }

        public bool HasConnection()
        {
            return Choices != null && Choices.Count != 0;
        }
    }
}
