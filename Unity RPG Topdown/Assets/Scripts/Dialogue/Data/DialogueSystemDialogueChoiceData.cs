using UnityEngine;

namespace DialogueSystem.Data
{
    using ScriptableObjects;
    using System;

    [Serializable]
    public class DialogueSystemDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueSystemDialogueSO NextDialogue { get; set; }

    }
}
