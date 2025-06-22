using System;

namespace NPCManagement
{
    [Serializable]
    public class NPCData
    {
        public string currentDialogueGroup;
        public bool isDead;

        public void ResetToDefault()
        {
            currentDialogueGroup = "InitialConversation";
            isDead = false;
        }
    }
}