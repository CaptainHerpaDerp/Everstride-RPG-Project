using Core.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/EquippableItem/Weapon")]
    public class WeaponItem : EquippableItem
    {
        [BoxGroup("Weapon Attributes")]
        public WeaponMode weaponMode;

        [BoxGroup("Weapon Attributes")]
        public WeaponType weaponType;

        bool isMelee => weaponMode != WeaponMode.Ranged && weaponMode != WeaponMode.Spell;
        bool notMelee => !isMelee;

        bool isSpell => weaponMode == WeaponMode.Spell;
        bool notSpell => !isSpell;

        [BoxGroup("Weapon Attributes")]
        public float weaponDamage;

        [ShowIf("isMelee"), BoxGroup("Weapon Attributes")]
        public float weaponRange;

        [ShowIf("isMelee"), BoxGroup("Weapon Attributes")]
        public float weaponAngle;

        [ShowIf("isMelee"), BoxGroup("Weapon Attributes")]
        public bool canBlock;

        [ShowIf("isSpell"), BoxGroup("Spell Attributes")]
        [field: SerializeField] private SpellType _spellType;

        [ShowIf("isSpell"), BoxGroup("Spell Attributes")]
        [field: SerializeField] private float _castCost;

        public SpellType SpellType
        {
            // If we try to retrieve this value while the weapon is not a spell, return an invalid value
            get
            {
                if (isSpell)
                {
                    return _spellType;
                }
                else
                {
                    Debug.LogError("This weapon is not a spell, it does not have a spell type.");
                    return _spellType;
                }
            }

            set
            {
                _spellType = value;
            }
        }

        public float CastCost
        {
            get
            {
                if (isSpell)
                {
                    return _castCost;
                }
                else
                {
                    Debug.LogError("This weapon is not a spell, it does not have a spell cost.");
                    return _castCost;
                }
            }

            set
            {
                _castCost = value;
            }
        }
    }
}