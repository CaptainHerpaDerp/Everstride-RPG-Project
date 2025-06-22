using System;
using UnityEngine;

/// <summary>
/// Stores all of the essential data to save a group.
/// </summary>
[Serializable]
public class BaseGroupSaveData
{
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public Vector2 Position { get; set; }
}
