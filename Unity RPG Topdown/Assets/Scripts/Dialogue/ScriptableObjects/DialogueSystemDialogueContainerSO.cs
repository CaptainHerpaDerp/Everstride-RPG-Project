using GraphSystem.Base.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    public class DialogueSystemDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<DialogueSystemDialogueGroupSO, List<BaseNodeSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<BaseNodeSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            DialogueGroups = new SerializableDictionary<DialogueSystemDialogueGroupSO, List<BaseNodeSO>>();
            UngroupedDialogues = new List<BaseNodeSO>();
        }

        public List<string> GetDialogueGroupNames()
        {
            List<string> dialogueGroupNames = new List<string>(); 

            foreach (DialogueSystemDialogueGroupSO dialogueGroup in DialogueGroups.Keys)
            {
                dialogueGroupNames.Add(dialogueGroup.GroupName);
            }

            return dialogueGroupNames;
        }

        public List<string> GetGroupedDialogueNames(DialogueSystemDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
        {
            List<BaseNodeSO> groupedDialogues = DialogueGroups[dialogueGroup];

            List<string> groupedDialogueNames = new List<string>();

            foreach (BaseNodeSO groupedDialogue in groupedDialogues)
            {
                if (groupedDialogue.GetType() == typeof(DialogueSystemDialogueSO))
                {
                    DialogueSystemDialogueSO dialogue = (DialogueSystemDialogueSO)groupedDialogue;

                    if (startingDialoguesOnly && !dialogue.IsStartingDialogue)
                    {
                        continue;
                    }

                    groupedDialogueNames.Add(dialogue.Text);

                }
            }

            return groupedDialogueNames;
        }

        public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();

            foreach (BaseNodeSO ungroupedDialogue in UngroupedDialogues)
            {
                if (ungroupedDialogue.GetType() == typeof(DialogueSystemDialogueSO))
                {
                    DialogueSystemDialogueSO dialogue = (DialogueSystemDialogueSO)ungroupedDialogue;

                    if (startingDialoguesOnly && !dialogue.IsStartingDialogue)
                    {
                        continue;
                    }

                    ungroupedDialogueNames.Add(dialogue.Text);
                }
            }

            return ungroupedDialogueNames;
        }
    }
}