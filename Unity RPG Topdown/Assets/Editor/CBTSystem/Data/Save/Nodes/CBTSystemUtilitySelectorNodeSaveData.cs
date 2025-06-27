
using UnityEngine;

namespace CBTSystem.Data.Save.Nodes
{
    /// <summary>
    /// Save data for the CBTSystemUtilitySelectorNode.
    /// </summary>
    /// <remarks>
    /// This class is used to save the state of the CBTSystemUtilitySelectorNode in the CBT system.
    /// </remarks>
    [System.Serializable]
    public class CBTSystemUtilitySelectorNodeSaveData : CBTSystemNodeSaveData
    {
        [field: SerializeField] public float Temperature { get; set; }
        [field: SerializeField] public float DecisionInterval { get; set; }
        [field: SerializeField] public float MinSwitchScore { get; set; }
        [field: SerializeField] public bool EmergencyOverride { get; set; }
        [field: SerializeField] public float StickyBonus { get; set; }

    }
}
