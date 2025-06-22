using System.Collections.Generic;
using System.Linq;

namespace QuestSystem
{
    public class OngoingQuest
    {
        public Quest quest;
        //public Dictionary<string, bool> completedQuestConditions = new();
        private List<QuestConditionData> questConditions = new();

        public List<QuestConditionData> QuestConditions => questConditions;

        public void ReInitialize()
        {
            questConditions = new();
        }

        public void AddQuestCondition(QuestConditionData questCondition)
        {
            questConditions.Add(questCondition);
        }

        public QuestConditionData GetQuestConditionDataFromVariable(string conditionVariable)
        {
            return questConditions.Find(x => x.ConditionVariable == conditionVariable);
        }

        public void MarkCondition(string conditionVariable, bool status)
        {
            var condition = GetQuestConditionDataFromVariable(conditionVariable);
            condition.CompletionStatus = status;
        }

        public OngoingQuest(Quest quest)
        {
            this.quest = quest;
        }

        public List<string> GetQuestConditionValues()
        {
            List<string> names = new();

            foreach (var questCondition in questConditions)
            {
                names.Add(questCondition.ConditionVariable);
            }

            return names;
        }

        public List<string> GetQuestConditionDisplayNames()
        {
            List<string> names = new();

            foreach (var questCondition in questConditions)
            {
                names.Add(questCondition.ConditionDisplayName);
            }

            return names;
        }

        public bool ContainConditionVariable(string conditionVariable)
        {
            return questConditions.Any(x => x.ConditionVariable == conditionVariable);
        }


        public bool IsObjectiveComplete()
        {
            foreach (var condition in questConditions)
            {
                // Return false if any of the values are false
                if (!condition.CompletionStatus == true)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class QuestConditionData
    {
        public string ConditionVariable;
        public bool CompletionStatus;
        public string ConditionDisplayName;
    }
}