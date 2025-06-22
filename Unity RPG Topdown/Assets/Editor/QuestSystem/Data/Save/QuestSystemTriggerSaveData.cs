using System;
using UnityEngine;

namespace QuestSystem.Data.Save
{
    [Serializable]
    public class QuestSystemTriggerSaveData
    {
        [field: SerializeField] public string ConditionKey { get; set; }
        [field: SerializeField] public bool ConditionValue { get; set; }
    }
}