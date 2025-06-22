
using GraphSystem.Base.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace CBTSystem.ScriptableObjects.Nodes
{
    public abstract class CBTSystemNodeSO : BaseNodeSO
    {
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public List<string> NextNodeIDs { get; set; }
        [field: SerializeField] public bool IsRootNode { get; set; }

        public virtual void Initialize(string nodeID, List<string> nextNodeIDs, bool isRootNode)
        {
            NodeID = nodeID;
            NextNodeIDs = nextNodeIDs;
            IsRootNode = isRootNode;
        }
    }
}