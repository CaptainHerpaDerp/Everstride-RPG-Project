
using System;
using UnityEngine;

namespace QuestSystem.Data.Save
{
    [Serializable]
    public class QuestSystemGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}