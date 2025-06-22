using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.Data.Save
{
    [Serializable]
    public class QuestSystemQuestTriggerData
    {
        [field: SerializeField] public string ConditionKey { get; set; }
        [field: SerializeField] public bool ConditionValue { get; set; }
    }
}
