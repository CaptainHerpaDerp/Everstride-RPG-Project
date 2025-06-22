namespace QuestSystem.Enumerations
{
    public enum QuestSystemNodeType
    {
        // Core quest flow nodes
        QuestStart,         // Node to trigger the start of a quest
        Objective,     // A node representing an objective (kill, collect, etc.)
        QuestCondition,     // A condition check node (item, location, or enemy state)
        QuestProgression,   // A node that links objectives or progression points
        QuestCompletion,    // Node that marks the quest as completed
        QuestFailure,       // Node for marking the quest as failed

        // Interaction and branching nodes
        DialogueInteraction, // Node to handle NPC/player dialogue or interactions
        BranchingDecision,   // A node for player decision-making (multiple quest paths)
        SubQuest,            // A node that links to another quest (sub-quest)

        // Time-based nodes
        Timer,              // A node with a timer or delay for quest events

        // Optional or special-purpose nodes
        RandomEvent,        // A node that triggers random quest events or outcomes
        RepeatableQuest,    // A node that resets or repeats the quest
        DynamicRewards,     // A node that calculates rewards dynamically
        AINPCBehavior,      // A node that triggers NPC behavior based on quest progress
    }

    public enum QuestCondition
    {
        KillTarget,
        CollectItem,
        PressKey,
        ActivateNPCDialogueStep
    }
}
