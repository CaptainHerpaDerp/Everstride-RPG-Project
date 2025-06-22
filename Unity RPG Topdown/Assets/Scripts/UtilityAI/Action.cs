using UnityEngine;

namespace UtilityAI.Core
{
    public abstract class Action : ScriptableObject
    {
        public string Name;
        private float score;

        public float Score
        {
            get { return score; }
            set
            { 
                this.score = Mathf.Clamp01(value);
            }
        }

        public Consideration[] considerations;

        public Transform RequiredDestination { get; protected set; }

        public virtual void Awake()
        {
            score = 0;
        }

        public abstract void Execute(NPCController npc);

        public abstract void SetRequiredDestination(NPCController npc);
    }
}
