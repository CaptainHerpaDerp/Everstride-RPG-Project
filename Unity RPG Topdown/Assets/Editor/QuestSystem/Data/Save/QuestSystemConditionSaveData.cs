using System;
using UnityEngine;

namespace QuestSystem.Data.Save
{
    using QuestSystem.Enumerations;

    [Serializable]
    public class QuestSystemConditionSaveData
    {
        [field: SerializeField] public QuestCondition Condition { get; set; }
        [field: SerializeField] public string ConditionValue { get; set; }
    }
}
