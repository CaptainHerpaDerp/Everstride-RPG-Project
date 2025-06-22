using UnityEngine;

namespace DialogueSystem
{
    using Abstracts;
    using ScriptableObjects;

    public class Dialogue : MonoBehaviour, IQuestHandler
    {
        [SerializeField] public DialogueSystemDialogueContainerSO dialogueContainer;
        [SerializeField] public DialogueSystemDialogueGroupSO dialogueGroup;
        [SerializeField] public DialogueSystemDialogueSO dialogue;

        [SerializeField] private bool groupedDialogues;
        [SerializeField] public bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] public int selectedDialogueGroupIndex;
        [SerializeField] public int selectedDialogueIndex;

        public void SetDialogueContainer(DialogueSystemDialogueContainerSO dialogueContainer)
        {
            this.dialogueContainer = dialogueContainer;
        }

        public DialogueSystemDialogueGroupSO GetGroupOfName(string name)
        {
            foreach (var item in dialogueContainer.DialogueGroups)
            {
                if (item.Key.name == name)
                {
                    return item.Key;
                }
            }

            throw new System.Exception("Dialogue Group not found");
        }

        public string GetCurrentDialogueGroupName()
        {
            return dialogueGroup.name;
        }

        public void SetCurrentDialogueGroup(string newGroup)
        {
            dialogueGroup = GetGroupOfName(newGroup);
        }

        public void UpdateCurrentDialogueGroup(string npcID, string dialogueGroupName)
        {
            throw new System.NotImplementedException();
        }
    }
}
