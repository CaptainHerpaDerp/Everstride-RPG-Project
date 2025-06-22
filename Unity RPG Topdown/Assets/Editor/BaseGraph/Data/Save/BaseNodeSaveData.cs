using System;
using UnityEngine;

namespace GraphSystem.Base.Data.Save
{
    /// <summary>
    /// A base node with all of the necessary data to save a node.
    /// </summary>

    [Serializable]
    public class BaseNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; } 
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}