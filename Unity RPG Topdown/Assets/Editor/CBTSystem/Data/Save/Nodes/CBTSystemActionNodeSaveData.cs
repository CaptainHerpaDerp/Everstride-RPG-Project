using CBTSystem.Enumerations;
using UnityEngine;

namespace CBTSystem.Data.Save.Nodes
{
    public class CBTSystemActionNodeSaveData : CBTSystemNodeSaveData
    {
        [field: SerializeField] public CBTActionType ActionType { get; set; }
    }
}