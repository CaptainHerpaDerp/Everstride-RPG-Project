using UnityEngine;
using UtilityAI.Core;

namespace UtilityAI.Actions
{
    [CreateAssetMenu(fileName = "Work", menuName = "UtilityAI/Actions/Work")]
    public class Work : Action
    {
        public override void Execute(NPCController npc)
        {
            npc.DoWork(3);
        }

        public override void SetRequiredDestination(NPCController npc)
        {
            RequiredDestination = GameObject.FindGameObjectWithTag("TreeResource").transform;
        }
    }
}
