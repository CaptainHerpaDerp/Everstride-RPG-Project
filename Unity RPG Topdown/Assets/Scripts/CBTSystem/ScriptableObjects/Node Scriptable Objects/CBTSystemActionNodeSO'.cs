using UnityEngine;

namespace CBTSystem.ScriptableObjects.Nodes
{

    using Enumerations;
    using System.Collections.Generic;

    public class CBTSystemActionNodeSO : CBTSystemNodeSO
    {
        [field: SerializeField] public CBTActionType ActionType { get; set; }

        public void Initialize(string nodeID, List<string> nextNodeIDs, bool isRootNode, CBTActionType actionType)
        {
            base.Initialize(nodeID, nextNodeIDs, isRootNode);    

            ActionType = actionType;
        }
    }
}