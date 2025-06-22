using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.ScriptableObjects
{
    using GraphSystem.Base.ScriptableObjects;
    using QuestSystem.Data.Save;
    using QuestSystem.Enumerations;

    public class QuestSystemQuestSO : BaseNodeSO
    {
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public string Description { get; set; }
        [field: SerializeField] public string NextNodeID { get; set; }
        [field: SerializeField] public List<QuestSystemQuestConditionData> Conditions { get; set; }
        [field: SerializeField] public List<QuestSystemQuestTriggerData> Triggers { get; set; }
        [field: SerializeField] public bool IsRootNode { get; set; }
        [field: SerializeField] public QuestSystemNodeType NodeType { get; set; }
        [field: SerializeField] public string NextGroupName { get; set; }

        public void Initialize(string nodeID, string description, string nextNodeID, List<QuestSystemQuestConditionData> questConditions, List<QuestSystemQuestTriggerData> questTriggers, QuestSystemNodeType nodeType, bool isRootNode, string newGroupNameField)
        {
            NodeID = nodeID;
            Description = description;
            NextNodeID = nextNodeID;
            Conditions = questConditions;
            Triggers = questTriggers;
            IsRootNode = isRootNode;
            NodeType = nodeType;
            NextGroupName = newGroupNameField;
        }

        public string GetCurrentObjective()
        {
            string returnString = "";

            foreach (var condition in Conditions)
            {
                switch (condition.ConditionType)
                {
                    case QuestCondition.KillTarget:
                        returnString += "Kill: ";
                        break;

                    case QuestCondition.CollectItem:
                        returnString += "Gather: ";
                        break;

                    case QuestCondition.PressKey:
                        returnString += "Press: ";
                        break;
                }

                returnString += condition.ConditionValue + "\n";
            }

            return returnString;
        }
    }
}

/*
             

       
 node.QuestName,
node.Conditions,
node.NodeType,
node.IsStartingNode(),
node.NewGroupNameField
            
 */
