using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    using GraphSystem.Base.Data.Save;
    using Enumerations;

    [Serializable]
    public class DialogueSystemNodeSaveData : BaseNodeSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DialogueSystemChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public DialogueSystemDiagType DialogueType { get; set; }

        // Add a property to store the event trigger information.
        [field: SerializeField] public List<DialogueSystemEventTriggerSaveData> EventTriggers { get; set; }
    }
}
