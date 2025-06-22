using System.Collections.Generic;
using UnityEngine;
using System;
using GraphSystem.Base.Data.Save;

namespace QuestSystem.Data.Save
{
    using Enumerations;

    [Serializable]
    public class QuestSystemNodeSaveData : BaseNodeSaveData
    {
        [field: SerializeField] public string NextNodeID { get; set; }
        [field: SerializeField] public string Description { get; set; }
        [field: SerializeField] public List<QuestSystemConditionSaveData> Conditions { get; set; }
        [field: SerializeField] public List<QuestSystemTriggerSaveData> Triggers { get; set; }
        [field: SerializeField] public QuestSystemNodeType NodeType { get; set; }
        [field: SerializeField] public string NextGroupName { get; set; }
    }
}