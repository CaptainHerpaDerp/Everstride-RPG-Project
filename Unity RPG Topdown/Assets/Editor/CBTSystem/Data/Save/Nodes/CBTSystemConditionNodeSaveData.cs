using UnityEngine;
using System.Collections.Generic;

namespace CBTSystem.Data.Save.Nodes
{
    using Elements;
    using Enumerations;

    public class CBTSystemConditionNodeSaveData : CBTSystemNodeSaveData
    {
        [field: SerializeField] public List<ConditionEntry> ConditionEntries;
        [field: SerializeField] public List<LogicalOperator> Connectors;
    }
}