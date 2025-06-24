using CBTSystem.Enumerations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBTSystem.ScriptableObjects.Nodes
{
    using Elements;

    public class CBTSystemConditionNodeSO : CBTSystemNodeSO
    {
        [field: SerializeField] public List<ConditionEntry> ConditionEntries;
        [field: SerializeField] public List<LogicalOperator> Connectors;
        [field: SerializeField] public int Priority {  get; set; }

        public void Initialize(string nodeID, List<string> nextNodeID, bool isRootNode, List<ConditionEntry> conditionEntries, List<LogicalOperator> logicalOperators, int priority)
        {
            base.Initialize(nodeID, nextNodeID, isRootNode);

            ConditionEntries = conditionEntries;
            Connectors = logicalOperators;
            Priority = priority;
        }

        /// <summary>
        /// Condition nodes should only have one output node, so we can just return the first one and skip node id checking
        /// </summary>
        /// <returns></returns>
        public string GetConnectedNode()
        {
            if (NextNodeIDs.Count == 0)
            {
                Debug.LogError(NodeID + " has no connected nodes!");

                return null;
            }

            // (this shouldn't be possible)
            if (NextNodeIDs.Count > 1)
            {
                Debug.LogError($"{NodeID} has more than one connected node!");
                foreach (string nodeID in NextNodeIDs) {
                    Debug.LogError(nodeID);
                }

                return null;
            }

            // Get the first node id
            return NextNodeIDs.First();
        }
    }
}