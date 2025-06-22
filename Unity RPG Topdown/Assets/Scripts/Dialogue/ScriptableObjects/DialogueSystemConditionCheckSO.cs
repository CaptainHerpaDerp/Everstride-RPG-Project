using DialogueSystem.Enumerations;
using GraphSystem.Base.ScriptableObjects;
using UnityEngine;

public class DialogueSystemConditionCheckSO : BaseNodeSO
{
    [field: SerializeField] public string ConditionKey { get; set; }
    [field: SerializeField] public bool ExpectedValue { get; set; }
    [field: SerializeField] public string ItemIDField { get; set; }
    [field: SerializeField] public ConditionCheckType ConditionCheckType { get; set; }
    [field: SerializeField] public BaseNodeSO ConnectedNode { get; set; }

    public void Initialize(string conditionKey, bool expectedValue, string itemIDField, ConditionCheckType conditionCheckType, BaseNodeSO connectedNode)
    {
        ConditionKey = conditionKey;
        ExpectedValue = expectedValue;
        ItemIDField = itemIDField;
        ConditionCheckType = conditionCheckType;
        ConnectedNode = connectedNode;
    }
}
