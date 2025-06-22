using DialogueSystem.Enumerations;
using GraphSystem.Base.Data.Save;
using System;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    /// <summary>
    /// Stores the data for a condition check node.
    /// </summary>
    [Serializable]
    public class DialogueSystemConditionCheckNodeSaveData : BaseNodeSaveData
    {
        [field: SerializeField] public string ConditionKey { get; set; }
        [field: SerializeField] public bool ExpectedValue { get; set; }
        [field: SerializeField] public string ItemIDField { get; set; }
        [field: SerializeField] public ConditionCheckType ConditionCheckType { get; set; }
        [field: SerializeField] public string ConnectedNodeID { get; set; } 
    }
}