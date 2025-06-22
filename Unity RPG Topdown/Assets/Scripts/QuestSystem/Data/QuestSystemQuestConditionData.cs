using System;
using UnityEngine;

namespace QuestSystem.Data.Save
{
    using QuestSystem.Enumerations;

    [Serializable]
    public class QuestSystemQuestConditionData
    {
        [field: SerializeField] public QuestCondition ConditionType { get; set; }
        [field: SerializeField] public string ConditionValue { get; set; }
        [field: SerializeField] public bool IsConditionMet { get; set; }
    }
}
