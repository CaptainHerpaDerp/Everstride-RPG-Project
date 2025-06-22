using UnityEngine;
using UtilityAI.Core;

namespace UtilityAI.Considerations
{
    [CreateAssetMenu(fileName = "HungerConsideration", menuName = "UtilityAI/Considerations/Hunger Consideration")]
    public class HungerConsideration : Consideration
    {
        public override float ScoreConsideration(NPCController npc)
        {
            Score = responseCurve.Evaluate(Mathf.Clamp01(npc.stats.Hunger / 100f));
            return Score;
        }
    }
}