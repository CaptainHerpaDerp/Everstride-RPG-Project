using System.Collections.Generic;
using UnityEngine;
using GraphSystem.Base.ScriptableObjects;

namespace QuestSystem.ScriptableObjects
{
    public class QuestSystemQuestContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<QuestSystemQuestGroupSO, List<BaseNodeSO>> QuestGroups { get; set; }
        [field: SerializeField] public List<QuestSystemQuestSO> UngroupedQuests { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            QuestGroups = new SerializableDictionary<QuestSystemQuestGroupSO, List<BaseNodeSO>>();
            UngroupedQuests = new List<QuestSystemQuestSO>();
        }

        public List<string> GetQuestGroupNames()
        {
            List<string> questGroupNames = new();

            foreach (var group in QuestGroups)
            {
                questGroupNames.Add(group.Key.name);
            }

            return questGroupNames;
        }

        public List<string> GetGroupedQuestNames(QuestSystemQuestGroupSO questGroup, bool startingQuestsOnly)
        {
            List<BaseNodeSO> groupedQuests = QuestGroups[questGroup];

            List<string> groupedQuestNamesList = new();

            foreach (BaseNodeSO groupedQuest in groupedQuests)
            {
                if (groupedQuest.GetType() == typeof(QuestSystemQuestSO))
                {
                    QuestSystemQuestSO quest = (QuestSystemQuestSO)groupedQuest;

                    if (startingQuestsOnly && !quest.IsRootNode)
                    {
                        continue;
                    }

                    groupedQuestNamesList.Add(quest.Description);
                }
            }

            return groupedQuestNamesList;
        }

        public List<string> GetUngroupedQuestNames(bool startingQuestsOnly)
        {
            List<string> ungroupedQuestNames = new();

            foreach (var ungroupedQuest in UngroupedQuests)
            {
                if (ungroupedQuest.GetType() == typeof(QuestSystemQuestSO))
                {
                    QuestSystemQuestSO quest = (QuestSystemQuestSO)ungroupedQuest;

                    if (startingQuestsOnly && !quest.IsRootNode)
                    {
                        continue;
                    }

                    ungroupedQuestNames.Add(quest.Description);
                }
            }

            return ungroupedQuestNames;
        }
    }
}
