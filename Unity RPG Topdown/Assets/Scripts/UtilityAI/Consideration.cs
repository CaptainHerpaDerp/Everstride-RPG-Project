using UnityEngine;

namespace UtilityAI.Core
{
    public abstract class Consideration : ScriptableObject
    {
        public string Name;
        private float score;

        [SerializeField] protected AnimationCurve responseCurve;

        public float Score
        {
            get { return score; }
            set
            {
                this.score = Mathf.Clamp01(value);
            }
        }

        public virtual void Awake()
        {
            score = 0;
        }

        public abstract float ScoreConsideration(NPCController npc);
    }
}

