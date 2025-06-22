using UnityEngine;

namespace CBTSystem.ScriptableObjects
{
    public class CBTSystemGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}
