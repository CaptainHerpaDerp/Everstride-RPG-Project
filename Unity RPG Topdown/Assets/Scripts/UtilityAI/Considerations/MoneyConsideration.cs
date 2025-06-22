using UnityEngine;
using UtilityAI.Core;

namespace UtilityAI.Considerations
{
    [CreateAssetMenu(fileName = "MoneyConsideration", menuName = "UtilityAI/Considerations/Money Consideration")]
    public class MoneyConsideration : Consideration
    {
        public override float ScoreConsideration(NPCController npc)
        {
            Score = responseCurve.Evaluate(Mathf.Clamp01(npc.stats.Money / 100f));
            return Score;
        }
    }
}