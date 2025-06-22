using UnityEngine;
using System;
using GraphSystem.Base.Data.Save;

namespace CBTSystem.Data.Save.Nodes
{
    using System.Collections.Generic;

    [Serializable]
    public abstract class CBTSystemNodeSaveData : BaseNodeSaveData
    {
        [field: SerializeField] public List<string> NextNodeIDs { get; set; }
        [field: SerializeField] public bool IsRootNode { get; set; }
    }
}