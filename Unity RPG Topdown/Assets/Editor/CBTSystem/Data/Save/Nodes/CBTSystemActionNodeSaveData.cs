using CBTSystem.Enumerations;
using System;
using UnityEngine;

namespace CBTSystem.Data.Save.Nodes
{
    [Serializable]
    public class CBTSystemActionNodeSaveData : CBTSystemNodeSaveData
    {
        [field: SerializeField] public CBTActionType ActionType { get; set; }
    }
}