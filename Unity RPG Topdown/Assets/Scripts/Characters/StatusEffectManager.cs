using UnityEngine;
using UnityEngine.U2D.Animation;
using Core.Enums;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager instance;

    [SerializeField] private SpriteLibraryAsset bleedingEffect, fearEffect, fireEffect, iceEffect, natureEffect, petrificationEffect, poisonEffect, shockEffect, sicknessEffect, sleepEffect, stunEffect;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one StatusEffectManager in the scene");
            Destroy(gameObject);
        }
    }

    public SpriteLibraryAsset GetLibraryAssetForEffect(StatusEffect effect)
    {
        switch (effect)
        {
            case StatusEffect.Bleeding:
                return bleedingEffect;
            case StatusEffect.Fear:
                return fearEffect;
            case StatusEffect.Fire:
                return fireEffect;
            case StatusEffect.Ice:
                return iceEffect;
            case StatusEffect.Nature:
                return natureEffect;
            case StatusEffect.Petrification:
                return petrificationEffect;
            case StatusEffect.Poison:
                return poisonEffect;
            case StatusEffect.Shock:
                return shockEffect;
            case StatusEffect.Sickness:
                return sicknessEffect;
            case StatusEffect.Sleep:
                return sleepEffect;
            case StatusEffect.Stun:
                return stunEffect;
            default:
                return null;
        }
    }
}
