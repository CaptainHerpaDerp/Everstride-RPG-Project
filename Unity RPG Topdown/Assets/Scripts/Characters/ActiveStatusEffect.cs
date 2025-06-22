using UnityEngine;
using Core.Enums;

namespace Characters
{
    public class ActiveStatusEffect
    {
        public ActiveStatusEffect(StatusEffect type, float duration, GameObject visualEffectObj)
        {
            this.type = type;
            this.duration = duration;
            if (visualEffectObj != null)
            {
                this.visualEffectObj = visualEffectObj;
            }
        }

        public StatusEffect type;
        public float duration;
        public GameObject visualEffectObj;
    }
}
