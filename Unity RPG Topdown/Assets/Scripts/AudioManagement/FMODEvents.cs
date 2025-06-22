using Core;
using Core.Enums;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioManagement
{
    public class FMODEvents : Singleton<FMODEvents>
    {
        [FoldoutGroup("Character Sounds")]
        [field: SerializeField, FoldoutGroup("Character Sounds/Human")] public EventReference humanHitSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Character Sounds/Orc")] public EventReference orcHitSound { get; private set; }

        #region Weapon Attack Sounds

        [FoldoutGroup("Weapon Attack Sounds")]
        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds/Slash")] public EventReference lightSlashSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds/Slash")] public EventReference mediumSlashSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds/Slash")] public EventReference heavySlashSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds")] public EventReference flailSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds")] public EventReference thrustSound { get; private set; }
 
        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds/Bow")] public EventReference bowDrawSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds/Bow")] public EventReference bowReleaseSound { get; private set; }


        [field: SerializeField, FoldoutGroup("Weapon Attack Sounds")]
        public EventReference metalShieldBlockSound { get; private set; }

        #endregion

        #region Spell Sounds

        [FoldoutGroup("Spell Sounds", Order = 1)]
        [InfoBox("Parent group for all spell sounds.", InfoMessageType.None)] // Optional visual indicator in the inspector
        [field: SerializeField]
        public bool showSpellSounds; // Optional toggle field to verify the group exists in Odin

        [FoldoutGroup("Spell Sounds/Fire", Order = 2)]
        [field: SerializeField, FoldoutGroup("Spell Sounds/Fire")] public EventReference fireCastSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Fire")] public EventReference fireHoldLoopSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Fire")] public EventReference fireThrowSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Fire")] public EventReference fireExplodeSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Fire")] public EventReference fireSpellShieldSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Spell Sounds/Ice")] public EventReference iceCastSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Ice")] public EventReference iceHoldLoopSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Ice")] public EventReference iceThrowSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Ice")] public EventReference iceHitSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Ice")] public EventReference iceExplodeSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Ice")] public EventReference iceSpellShieldSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Spell Sounds/Air")] public EventReference airCastSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Air")] public EventReference airHoldLoopSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Air")] public EventReference airThrowSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Air")] public EventReference airHitSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Air")] public EventReference airExplodeSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Air")] public EventReference airSpellShieldSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Spell Sounds/Earth")] public EventReference earthCastSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Earth")] public EventReference earthHoldLoopSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Earth")] public EventReference earthThrowSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Earth")] public EventReference earthHitSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Earth")] public EventReference earthExplodeSound { get; private set; }
        [field: SerializeField, FoldoutGroup("Spell Sounds/Earth")] public EventReference earthSpellShieldSound { get; private set; }

        public EventReference GetCorrespondingSpellCastSound(SpellType type)
        {
            switch (type)
            {
                case SpellType.Fire:
                    return fireCastSound;
                case SpellType.Ice:
                    return iceCastSound;
                case SpellType.Wind:
                    return airCastSound;
                case SpellType.Earth:
                    return earthCastSound;
            }

            Debug.LogError("No corresponding spell cast sound found for " + type);
            return fireCastSound;
        }

        public EventReference GetCorrespondingSpellHoldLoopSound(SpellType type)
        {
            switch (type)
            {
                case SpellType.Fire:
                    return fireHoldLoopSound;
                case SpellType.Ice:
                    return iceHoldLoopSound;
                case SpellType.Wind:
                    return airHoldLoopSound;
                case SpellType.Earth:
                    return earthHoldLoopSound;
            }

            Debug.LogError("No corresponding spell hold loop sound found for " + type);
            return fireHoldLoopSound;
        }

        public EventReference GetCorrespondingSpellThrowSound(SpellType type)
        {
            switch (type)
            {
                case SpellType.Fire:
                    return fireThrowSound;
                case SpellType.Ice:
                    return iceThrowSound;
                case SpellType.Wind:
                    return airThrowSound;
                case SpellType.Earth:
                    return earthThrowSound;
            }

            Debug.LogError("No corresponding spell throw sound found for " + type);
            return fireThrowSound;
        }

        public EventReference GetCorrespondingSpellExplodeSound(SpellType type)
        {
            switch (type)
            {
                case SpellType.Fire:
                    return fireExplodeSound;
                case SpellType.Ice:
                    return iceExplodeSound;
                case SpellType.Wind:
                    return airExplodeSound;
                case SpellType.Earth:
                    return earthExplodeSound;
            }

            Debug.LogError("No corresponding spell explode sound found for " + type);
            return fireExplodeSound;
        } 

        #endregion

        #region

        [field: SerializeField, FoldoutGroup("Misc Sounds")]
        public EventReference doorOpenSound { get; private set; }

        [field: SerializeField, FoldoutGroup("Misc Sounds")]
        public EventReference doorCloseSound { get; private set; }

        [FoldoutGroup("Enemy Sounds")]

        [field: SerializeField, FoldoutGroup("Enemy Sounds")]
        public EventReference wolfGrowlSound { get; private set; }

        #endregion

        #region UI Sounds

        [field: SerializeField, FoldoutGroup("UI Sounds")]
        public EventReference equipSound { get; private set; }

        [field: SerializeField, FoldoutGroup("UI Sounds")]
        public EventReference unequipSound { get; private set; }

        [field: SerializeField, FoldoutGroup("UI Sounds")]
        public EventReference hoverSound { get; private set; }

        [field: SerializeField, FoldoutGroup("UI Sounds")]
        public EventReference menuOpenSound { get; private set; }

        [field: SerializeField, FoldoutGroup("UI Sounds")]
        public EventReference menuCloseSound { get; private set; }

        #endregion
    }
}
