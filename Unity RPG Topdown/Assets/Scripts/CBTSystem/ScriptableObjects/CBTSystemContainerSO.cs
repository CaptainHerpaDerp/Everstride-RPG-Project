using CBTSystem.ScriptableObjects;
using CBTSystem.ScriptableObjects.Nodes;
using GraphSystem.Base.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

public class CBTSystemContainerSO : ScriptableObject
{
    [field: SerializeField] public string FileName { get; set; }
    [field: SerializeField] public SerializableDictionary<CBTSystemGroupSO, List<CBTSystemNodeSO>> Groups { get; set; }
    [field: SerializeField] public List<CBTSystemNodeSO> UngroupedNodes { get; set; }

    public void Initialize(string fileName)
    {
        FileName = fileName;

        Groups = new();
        UngroupedNodes = new();
    }
}
