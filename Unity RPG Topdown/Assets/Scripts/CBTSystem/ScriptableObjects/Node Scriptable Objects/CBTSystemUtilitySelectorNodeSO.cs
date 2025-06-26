using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CBTSystem.ScriptableObjects.Nodes
{
    public class CBTSystemUtilitySelectorNodeSO : CBTSystemNodeSO
    {
        public override void Initialize(string nodeID, List<string> nextNodeIDs, bool isRootNode)
        {
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