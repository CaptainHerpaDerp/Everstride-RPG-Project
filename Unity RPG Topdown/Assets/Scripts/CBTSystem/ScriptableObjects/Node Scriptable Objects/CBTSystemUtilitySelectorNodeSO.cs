using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBTSystem.ScriptableObjects.Nodes
{
    public class CBTSystemUtilitySelectorNodeSO : CBTSystemNodeSO
    {
        [field: SerializeField] public float Temperature { get; set; }
        [field: SerializeField] public float DecisionInterval { get; set; }
        [field: SerializeField] public float MinSwitchScore { get; set; }
        [field: SerializeField] public bool EmergencyOverride { get; set; }
        [field: SerializeField] public float StickyBonus { get; set; }

        // Track the score of the current node so it can be compared to new candidates
        public float CurrentActionNodeScore;

        public void Initialize(string nodeID, List<string> nextNodeIDs, bool isRootNode, float temperature, float decisionInterval, float minSwitchScore, bool emergencyOverride, float sickyBonus)
        {
            Temperature = temperature;
            DecisionInterval = decisionInterval;
            MinSwitchScore = minSwitchScore;
            EmergencyOverride = emergencyOverride;
            StickyBonus = sickyBonus;

            base.Initialize(nodeID, nextNodeIDs, isRootNode);
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
                foreach (string nodeID in NextNodeIDs)
                {
                    Debug.LogError(nodeID);
                }

                return null;
            }

            // Get the first node id
            return NextNodeIDs.First();
        }
    }
}