using UnityEngine;
using UtilityAI.Core;

namespace UtilityAI.Actions
{
    [CreateAssetMenu(fileName = "Eat", menuName = "UtilityAI/Actions/Eat")]
    public class Eat : Action
    {
        public override void Execute(NPCController npc)
        {
            Debug.Log("Ate Food!");

            npc.stats.Hunger -= 30;
            npc.stats.Money -= 10;

            // Logic for updating the NPC's hunger

            npc.OnFinishedAction();
        }

        public override void SetRequiredDestination(NPCController npc)
        {
            RequiredDestination = GameObject.FindGameObjectWithTag("FoodPoint").transform;
        }
    }
}
