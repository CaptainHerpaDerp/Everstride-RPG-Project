using UnityEngine;
using UtilityAI.Core;

namespace UtilityAI.Considerations
{
    [CreateAssetMenu(fileName = "EnergyConsideration", menuName = "UtilityAI/Considerations/Energy Consideration")]
    public class EnergyConsideration : Consideration
    {
        public override float ScoreConsideration(NPCController npc)
        {
            Score = responseCurve.Evaluate(Mathf.Clamp01(npc.stats.Energy / 100f));
            return Score;
        }
    }
}
