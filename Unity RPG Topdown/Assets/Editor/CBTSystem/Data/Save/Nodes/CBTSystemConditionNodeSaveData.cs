using System.Collections.Generic;
using UnityEngine;

namespace CBTSystem.Data.Save.Nodes
{
    using Elements;
    using Enumerations;
    using System;

    [Serializable]

    public class CBTSystemConditionNodeSaveData : CBTSystemNodeSaveData
    {
        [field: SerializeField] public List<ConditionEntry> ConditionEntries;
        [field: SerializeField] public List<LogicalOperator> Connectors;
        [field: SerializeField] public int Priority { get; set; }
    }
}