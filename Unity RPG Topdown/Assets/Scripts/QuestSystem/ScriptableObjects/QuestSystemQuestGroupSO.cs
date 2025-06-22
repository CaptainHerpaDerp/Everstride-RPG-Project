using UnityEngine;

namespace QuestSystem.ScriptableObjects
{
    public class QuestSystemQuestGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}
