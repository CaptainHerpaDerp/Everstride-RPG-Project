using DialogueSystem.Enumerations;
using System;
using UnityEngine;

namespace DialogueSystem.Data.Save
{

    [Serializable]
    public class DialogueSystemEventTriggerSaveData
    {
        [field: SerializeField] public DialogueSystemDiagEvent TriggerType { get; set; }
        [field: SerializeField] public object TriggerValue { get; set; }
        [field: SerializeField] public string ScriptableObjectGUID { get; set; }

    }
}