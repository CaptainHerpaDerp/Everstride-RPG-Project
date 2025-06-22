using System;
using UnityEngine;
using GraphSystem.Base.Data.Save;

namespace DialogueSystem.Data.Save
{
    [Serializable]
    public class DialogueSystemChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }

    }
}
