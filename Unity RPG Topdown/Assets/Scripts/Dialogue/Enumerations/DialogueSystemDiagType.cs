using UnityEngine;

namespace DialogueSystem.Enumerations
{
    public enum DialogueSystemDiagType
    {
        SingleChoice,
        MultipleChoice,
        ConditionCheck
    }

    public enum DialogueSystemDiagEvent
    {
        None,
        ShopOpen,
        StartQuest,
        SetGroup,
        RemovePlayerItem
    }

    public enum ConditionCheckType
    {
        CheckGameState,
        HasItem,
    }
}
