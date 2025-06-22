using System.Collections.Generic;
using UnityEngine;

namespace GraphSystem.Base.Data.Save
{
    public class BaseGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<BaseGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<BaseNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupNodeNames { get; set; }

        [field: SerializeField] public string SerializedNodes { get; set; }

        public List<NodeLinkData> NodeLinks = new();

        public void Initialize(string fileName)
        {
            FileName = fileName;
            Groups = new();
            Nodes = new();
        }
    }
}
